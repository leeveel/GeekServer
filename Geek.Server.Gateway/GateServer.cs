using Geek.Server.Core.Actors;
using Geek.Server.Core.Net;
using Geek.Server.Core.Net.Kcp;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Gateway.Common;
using Geek.Server.Gateway.Outer;
using Microsoft.AspNetCore.Connections;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Geek.Server.Gateway
{
    public class GateServer : Singleton<GateServer>
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger(); 

        internal class GateTcpConnectHander : ConnectionHandler
        {
            readonly ConcurrentDictionary<long, BaseNetChannel> outerChannelMap = Instance.outerChannelMap;
            readonly UdpServer innerUdpServer = Instance.innerUdpServer;

            public override async Task OnConnectedAsync(ConnectionContext connection)
            {
                string remoteAdd = connection.RemoteEndPoint.ToString();
                LOGGER.Info($"tcp连接:{remoteAdd}");
                 
                try
                {
                    if (!await CheckLoad(connection))
                    {
                        await Task.Delay(2000);
                        return;
                    }
                }
                catch
                {
                    return;
                }

                TcpChannel channel = null;
                try
                {
                    channel = new TcpChannel(connection, onRecv)
                    {
                        RemoteAddress = remoteAdd
                    };
                    await channel.StartAsync();
                }
                catch { }
                finally
                {
                    LOGGER.Info($"tcp连接关闭:{remoteAdd}");
                    if (channel != null && channel.NetId != 0)
                        outerChannelMap.TryRemove(channel.NetId, out _);
                }
            }

            private async Task<bool> CheckLoad(ConnectionContext context)
            {
                if (Instance.CurActiveChannelCount < Settings.InsAs<GateSettings>().MaxClientCount)
                {
                    return true;
                }

                var writer = context.Transport.Output;
                var newGate = GatewayDiscoveryClient.Instance.GetIdleGateway();
                if (newGate != null && !string.IsNullOrEmpty(newGate.PublicIp))
                {
                    var data = MessagePack.MessagePackSerializer.Serialize(new Dictionary<string, string>
                    {
                        { "ip",newGate.PublicIp},
                        { "port",newGate.OuterPort.ToString()}
                    });
                    writer.WritePipe(new TempNetPackage(NetPackageFlag.NO_GATE_CONNECT, 0, 0, data));
                }
                else
                {
                    writer.WritePipe(new TempNetPackage(NetPackageFlag.NO_GATE_CONNECT, 0, 0));
                }
                await writer.FlushAsync();
                return false;
            }

            void onRecv(TcpChannel channel, TempNetPackage package)
            {
                LOGGER.Info($"收到tcp:package.flag :{package.ToString()}");
                long netId = package.netId;
                //检查网络id是否合格
                if (netId != 0)
                {
                    if (IdGenerator.GetActorType(netId) != ActorType.Gate || channel.NetId != 0 && channel.NetId != netId)
                    {
                        LOGGER.Warn($"OnTcpRecvOuter不正确的gate net id:{channel.NetId} {netId}");
                        package.flag = NetPackageFlag.CLOSE;
                        channel.Write(package);
                        return;
                    }
                }

                //内网服务器信息
                var server = GatewayDiscoveryClient.Instance.GetServer(package.innerServerId, ServerType.Game);
                if (server == null)
                {
                    LOGGER.Warn($"不能发现内部服务id:{package.innerServerId}");
                    package.flag = NetPackageFlag.NO_INNER_SERVER;
                    channel.Write(package);
                    return;
                }
                else if (server.InnerEndPoint == null)
                {
                    LOGGER.Warn($"服务器InnerEndPoint为空:{package.innerServerId}");
                    package.flag = NetPackageFlag.NO_INNER_SERVER;
                    channel.Write(package);
                    return;
                }

                switch (package.flag)
                {
                    case NetPackageFlag.SYN:
                        {
                            //LOGGER.Info($"收到tcp连接请求 netid:{netId}");
                            if (netId == 0)
                            {
                                netId = IdGenerator.GetActorID(ActorType.Gate);
                            }
                            else
                            {
                                if (outerChannelMap.TryGetValue(netId, out var cacheChannel))
                                {
                                    if (cacheChannel != channel)
                                    {
                                        cacheChannel?.Close();
                                        outerChannelMap.TryRemove(netId, out _); 
                                    }
                                }
                            }

                            //TODO 如果当前网关连接数量已经够多，则给客户端一个推荐网关 
                            channel.NetId = netId;
                            outerChannelMap[netId] = channel;
                            channel.TargetServerId = package.innerServerId;
                            channel.UpdateRecvMessageTime(TimeSpan.FromSeconds(5).Ticks);
                            //通知内部服务器   
                            package.netId = netId;
                            package.body = Encoding.UTF8.GetBytes(channel.RemoteAddress);
                            innerUdpServer.SendTo(package, server.InnerEndPoint);
                            break;
                        }

                    case NetPackageFlag.HEART:
                        //LOGGER.Info($"kcpservice recv GATE_HEART: {netId}");
                        channel.UpdateRecvMessageTime();
                        channel.Write(package);
                        innerUdpServer.SendTo(package, server.InnerEndPoint);
                        break;

                    case NetPackageFlag.MSG:
                        channel.UpdateRecvMessageTime();
                        innerUdpServer.SendTo(package, server.InnerEndPoint);
                        break;

                    case NetPackageFlag.CLOSE: 
                        channel.Close();
                        break;
                }
            }
        }

        TcpServer<GateTcpConnectHander> outerTcpServer;
        UdpServer outerUdpServer;
        UdpServer innerUdpServer;

        readonly ConcurrentDictionary<long, BaseNetChannel> outerChannelMap = new();
        public int CurActiveChannelCount { get; private set; } = 0; 
        GateServer() { }

        public void Start()
        {
            _ = GatewayDiscoveryClient.Instance.Start();
            var setting = Settings.Ins as GateSettings;
            innerUdpServer = new UdpServer(setting.InnerPort, OnUdpRecvInner);
            outerUdpServer = new UdpServer(setting.OuterPort, OnUdpRecvOuter);
            outerTcpServer = new();
            _ = innerUdpServer.Start();
            _ = outerUdpServer.Start();
            _ = outerTcpServer.Start(setting.OuterPort);
            _ = CheckTimeOut();
            _ = PrintInfo();
        }

        async Task PrintInfo()
        {
            while (!Settings.Ins.AppExitToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(3));
                LOGGER.Info($"channel总数:{outerChannelMap.Count},有效channel数量:{CurActiveChannelCount}");
            }
        }

       
        async Task CheckTimeOut()
        {
            var removeList = new List<long>();
            while (!Settings.Ins.AppExitToken.IsCancellationRequested)
            {
                removeList.Clear();
                int activeChannelCount = 0;
                var timeNow = DateTime.UtcNow;
                foreach (var kv in outerChannelMap)
                {
                    var netId = kv.Key;
                    var chann = kv.Value;
                    if (chann is UdpChannel udpChannel)
                    {
                        //这里只检测udp,tcp关闭的时候会自动移除
                        if (udpChannel.IsClose() || udpChannel.GetLastMessageTimeSecond(timeNow) > 12)
                        {
                            udpChannel.Close();
                            removeList.Add(netId);
                        }
                        else
                        {
                            activeChannelCount++;
                        }
                    }
                    else if (!chann.IsClose())
                    {
                        activeChannelCount++;
                    }
                }

                CurActiveChannelCount = activeChannelCount;

                foreach (var k in removeList)
                {
                    //LOGGER.Info($"超时移除:{k}");
                    outerChannelMap.TryRemove(k, out _);
                }
                await Task.Delay(1500);
            }
        }

        private bool CheckLoad(EndPoint endPoint)
        {
            if (CurActiveChannelCount < Settings.InsAs<GateSettings>().MaxClientCount)
            {
                return true;
            }
            var newGate = GatewayDiscoveryClient.Instance.GetIdleGateway();
            if (newGate != null && !string.IsNullOrEmpty(newGate.PublicIp))
            {
                var data = MessagePack.MessagePackSerializer.Serialize(new Dictionary<string, string>
                    {
                        { "ip",newGate.PublicIp},
                        { "port",newGate.OuterPort.ToString()}
                    });
                outerUdpServer.SendTo(new TempNetPackage(NetPackageFlag.NO_GATE_CONNECT, 0, 0, data), endPoint);
            }
            else
            {
                outerUdpServer.SendTo(new TempNetPackage(NetPackageFlag.NO_GATE_CONNECT, 0, 0), endPoint);
            }
            return false;
        }

        /// <summary>
        /// 接收内部服务器消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="point"></param>
        void OnUdpRecvInner(TempNetPackage package, EndPoint endPoint)
        {
            long netId = package.netId;
            int serverId = package.innerServerId;

            //LOGGER.Info($"收到内网包:{package.ToString()}");
            //检查网络id是否合格
            // TODO 下面步骤返回的话 需要给内服服务器返回消息，避免内部kcp一直重发
            if (netId != 0)
            {
                if (IdGenerator.GetActorType(netId) != ActorType.Gate)
                {
                    LOGGER.Warn($"OnUdpRecvInner不正确的gate net id:{netId}");
                    return;
                }
            }

            outerChannelMap.TryGetValue(netId, out var clientChannel);
            if (clientChannel == null)
            {
                innerUdpServer.SendTo(new TempNetPackage(NetPackageFlag.NO_GATE_CONNECT, netId, serverId), endPoint);
                return;
            }
            else if (clientChannel.IsClose())
            {
                outerChannelMap.TryRemove(netId, out _);
                innerUdpServer.SendTo(new TempNetPackage(NetPackageFlag.NO_GATE_CONNECT, netId, serverId), endPoint);
                return;
            }

            switch (package.flag)
            {
                case NetPackageFlag.ACK:
                    { 
                        clientChannel.Write(package);
                        break;
                    }

                case NetPackageFlag.MSG: 
                    clientChannel.Write(package);
                    break;

                case NetPackageFlag.CLOSE:
                    outerChannelMap.Remove(netId, out _);
                    clientChannel.Write(package);
                    clientChannel.Close();
                    LOGGER.Info($"kcpservice recv fin: {netId}");
                    break;
            }
        }

        /// <summary>
        /// 接收玩家udp消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param> 
        void OnUdpRecvOuter(TempNetPackage package, EndPoint endPoint)
        {
            long netId = package.netId;
            //检查网络id是否合格
            if (netId != 0)
            {
                if (IdGenerator.GetActorType(netId) != ActorType.Gate)
                {
                    LOGGER.Warn($"OnUdpRecvOuter不正确的gate net id:{netId}");
                    return;
                }
            }

            //检查内网服务器id
            if (package.innerServerId == 0)
            {
                LOGGER.Warn($"不正确的内网服id");
                outerUdpServer.SendTo(new TempNetPackage(NetPackageFlag.NO_INNER_SERVER, netId, 0), endPoint);
                return;
            }

            //LOGGER.Info($"收到外部udp包:{package.ToString()}");

            BaseNetChannel channel = null;
            if (netId != 0)
            {
                outerChannelMap.TryGetValue(netId, out channel);
                if (package.flag == NetPackageFlag.MSG || package.flag == NetPackageFlag.HEART)
                    if (channel == null || channel is not UdpChannel || channel.IsClose() || channel.TargetServerId != package.innerServerId)
                    {
#if DEBUG
                        LOGGER.Warn($"当前接收到来自netid:{netId}的udp消息，但channel不是udpchannel或者已经关闭，或者innerServerId不匹配...");
#endif
                        package.flag = NetPackageFlag.NO_GATE_CONNECT;
                        package.body = Span<byte>.Empty;
                        outerUdpServer.SendTo(package, endPoint);
                        outerChannelMap.Remove(netId, out _);
                        return;
                    }
            }

            //内网服务器信息
            var server = GatewayDiscoveryClient.Instance.GetServer(package.innerServerId, ServerType.Game);
            if (server == null)
            {
                outerChannelMap.Remove(netId, out _);
                outerUdpServer.SendTo(new TempNetPackage(NetPackageFlag.NO_INNER_SERVER, netId, package.innerServerId), endPoint);
                return;
            }
            else if (server.InnerEndPoint == null)
            {
                LOGGER.Warn($"服务器InnerEndPoint为空:{package.innerServerId}");
                package.flag = NetPackageFlag.NO_INNER_SERVER;
                channel.Write(package);
                return;
            }

            switch (package.flag)
            {
                case NetPackageFlag.SYN:
                    {
#if DEBUG
                        LOGGER.Info($"收到udp连接请求 netid:{package.ToString()}");
#endif
                        if (!CheckLoad(endPoint))
                        {
                            break;
                        }

                        if (netId == 0)
                        {
                            netId = IdGenerator.GetActorID(ActorType.Gate);
                        }
                        else
                        {
                            package.flag = NetPackageFlag.SYN_OLD_NET_ID;
                            if (outerChannelMap.TryGetValue(netId, out channel))
                            {
                                if (channel is not UdpChannel || channel.TargetServerId != package.innerServerId)
                                {
                                    channel?.Close();
                                    outerChannelMap.TryRemove(netId, out _);
                                    channel = null;
                                }
                            }
                        }

                        //TODO 如果当前网关连接数量已经够多，则给客户端一个推荐网关
                        if (channel == null)
                        {
                            channel = new UdpChannel(netId, package.innerServerId, outerUdpServer, endPoint);
                            outerChannelMap.TryAdd(channel.NetId, channel);
                        }
                        (channel as UdpChannel).UpdateRemoteAddress(endPoint);
                        //通知内部服务器   
                        package.netId = netId;
                        package.body = Encoding.UTF8.GetBytes(endPoint?.ToString() ?? "");
                        innerUdpServer.SendTo(package, server.InnerEndPoint);
                        break;
                    }

                case NetPackageFlag.HEART:
                    //LOGGER.Info($"kcpservice recv GATE_HEART: {netId}");
                    channel.UpdateRecvMessageTime();
                    channel.Write(package);
                    innerUdpServer.SendTo(package, server.InnerEndPoint);
                    break;

                case NetPackageFlag.MSG:
                    channel.UpdateRecvMessageTime();
                    innerUdpServer.SendTo(package, server.InnerEndPoint);
                    break;

                case NetPackageFlag.CLOSE: 
                    if (channel is UdpChannel)
                    {
                        channel.Close();
                        innerUdpServer.SendTo(package, server.InnerEndPoint);
                        LOGGER.Info($"kcpservice recv fin: {netId}");
                    }
                    break;
            }
        }

        public async Task Stop()
        {
            innerUdpServer.Close();
            outerUdpServer.Close();
            await outerTcpServer.Stop();
        }
    }
}
