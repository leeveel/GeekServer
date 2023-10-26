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
            readonly UdpServer innerUdpServer = Instance.innerUdpServer;

            public override async Task OnConnectedAsync(ConnectionContext context)
            {
                string remoteAdd = context.RemoteEndPoint.ToString();
                LOGGER.Info($"tcp连接:{remoteAdd}");
                 
                try
                {
                    if (!await CheckLoad(context))
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
                    channel = new TcpChannel(context, onRecv)
                    {
                        RemoteAddress = remoteAdd
                    };
                    await channel.StartAsync();
                }
                catch { }
                finally
                {
                    Instance.TryRemoveChannel(channel);
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
                            channel.NetId = netId;
                            channel.TargetServerId = package.innerServerId;
                            channel.UpdateRecvMessageTime(TimeSpan.FromSeconds(5).Ticks);

                            Instance.AddChannel(channel);

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
            var removeList = new List<BaseNetChannel>();
            while (!Settings.Ins.AppExitToken.IsCancellationRequested)
            {
                removeList.Clear();
                int activeChannelCount = 0;
                string activeConnectInfos = "\n";
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
                            removeList.Add(udpChannel);
                        }
                        else
                        {
                            activeChannelCount++;
#if DEBUG
                            activeConnectInfos += $"udp:{chann.RemoteAddress} \n";
#endif
                        }
                    }
                    else if (!chann.IsClose())
                    {
                        activeChannelCount++;
#if DEBUG
                        activeConnectInfos += $"tcp:{chann.RemoteAddress} \n";
#endif
                    }
                }

                CurActiveChannelCount = activeChannelCount; 

                int removeCount = 0;
                foreach (var k in removeList)
                {
                    TryRemoveChannel(k);
                    removeCount++;
                    if (removeCount > 10)
                    {
                        await Task.Delay(10);
                        removeCount = 0;
                    }
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


        public void TryRemoveChannel(BaseNetChannel channel)
        {
            if (channel == null || channel.NetId == 0)
                return;
            var id = channel.NetId;
            lock (outerChannelMap)
            {
                if (outerChannelMap.TryGetValue(id, out var cacheChannel))
                {
                    if (cacheChannel == channel)
                    {
#if DEBUG
                        LOGGER.Warn($"移除channel:{channel.RemoteAddress} {channel.GetType().Name}");
#endif
                        outerChannelMap.TryRemove(id, out _);
                        channel?.Close();
                    }
                }
            }
        }

        public void AddChannel(BaseNetChannel channel)
        {
            if (channel == null || channel.NetId == 0)
                return;
            lock (outerChannelMap)
            {
                var id = channel.NetId;
                if (outerChannelMap.TryGetValue(id, out var cacheChannel))
                {
                    cacheChannel?.Close();
                }
#if DEBUG
                LOGGER.Warn($"添加channel:{channel.RemoteAddress} {channel.GetType().Name}");
#endif
                outerChannelMap[channel.NetId] = channel;
            }
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
            if (clientChannel == null || clientChannel.IsClose())
            {
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
                if (outerChannelMap.TryGetValue(netId, out channel))
                {
                    if (channel.TargetServerId != package.innerServerId)
                    {
                        LOGGER.Warn($"当前包存在的channel serverid不匹配{channel.TargetServerId} ：{package.innerServerId}");
                        package.flag = NetPackageFlag.NO_GATE_CONNECT;
                        package.body = Span<byte>.Empty;
                        outerUdpServer.SendTo(package, endPoint);
                        channel.Close();
                        return;
                    }
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
                        }
                         
                        channel = new UdpChannel(netId, package.innerServerId, outerUdpServer, endPoint);
                        (channel as UdpChannel).UpdateRemoteAddress(endPoint);
                        AddChannel(channel);
                        //通知内部服务器   
                        package.netId = netId;
                        package.body = Encoding.UTF8.GetBytes(endPoint?.ToString() ?? "");
                        innerUdpServer.SendTo(package, server.InnerEndPoint);
                        break;
                    }

                case NetPackageFlag.HEART:
                    if (channel != null && channel is UdpChannel)
                    {
                        channel.UpdateRecvMessageTime();
                        channel.Write(package);
                        innerUdpServer.SendTo(package, server.InnerEndPoint);
                    }
                    break;

                case NetPackageFlag.MSG:
                    if (channel != null && channel is UdpChannel)
                    {
                        channel.UpdateRecvMessageTime();
                        innerUdpServer.SendTo(package, server.InnerEndPoint);
                    }
                    break;

                case NetPackageFlag.CLOSE:
                    if (channel != null && channel is UdpChannel)
                    {
                        TryRemoveChannel(channel);
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
