using Geek.Server.Core.Actors;
using Geek.Server.Core.Net;
using Geek.Server.Core.Net.Kcp;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Utils;
using Geek.Server.Gateway.Common;
using Geek.Server.GatewayKcp.Outer;
using Microsoft.AspNetCore.Connections;
using System.Collections.Concurrent;
using System.Net;

namespace Geek.Server.GatewayKcp
{
    public class GateServer : Singleton<GateServer>
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        internal class GateTcpConnectHander : ConnectionHandler
        {
            readonly ConcurrentDictionary<long, BaseNetChannel> outerChannelMap = GateServer.Instance.outerChannelMap;
            readonly UdpServer innerUdpServer = GateServer.Instance.innerUdpServer;

            TcpChannel channel = null;

            public override async Task OnConnectedAsync(ConnectionContext connection)
            {
                LOGGER.Info($"tcp连接:{connection.RemoteEndPoint}");
                channel = new TcpChannel(connection, onRecv);
                await channel.StartAsync();
                LOGGER.Info($"tcp连接关闭:{connection.RemoteEndPoint}");
                GateServer.Instance.outerChannelMap.TryRemove(channel.NetId, out _);
            }

            void onRecv(TempNetPackage package)
            {
                //LOGGER.Info($"收到tcp:package.flag :{package.ToString()}");
                long netId = package.netId;
                //检查网络id是否合格
                if (netId != 0)
                {
                    if (IdGenerator.GetActorType(netId) != ActorType.Gate)
                    {
                        LOGGER.Warn($"OnTcpRecvOuter不正确的gate net id:{netId}");
                        package.flag = NetPackageFlag.NO_GATE_CONNECT;
                        channel.Write(package);
                        return;
                    }
                }

                //检查内网服务器id
                if (package.innerServerId == 0)
                {
                    LOGGER.Warn($"不正确的内网服id");
                    package.flag = NetPackageFlag.NO_GATE_CONNECT;
                    channel.Write(package);
                    return;
                } 

                //内网服务器信息
                var server = ServerList.Instance.GetServer(package.innerServerId, ServerType.Game);
                if (server == null)
                {
                    package.flag = NetPackageFlag.CLOSE;
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
                                        channel = null;
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
                            innerUdpServer.SendTo(package, server.InnerUdpEndPoint);
                            break;
                        }

                    case NetPackageFlag.GATE_HEART:
                        //LOGGER.Info($"kcpservice recv GATE_HEART: {netId}");
                        channel.UpdateRecvMessageTime();
                        channel.Write(package);
                        innerUdpServer.SendTo(package, server.InnerUdpEndPoint);
                        break;

                    case NetPackageFlag.MSG:
                        channel.UpdateRecvMessageTime();
                        innerUdpServer.SendTo(package, server.InnerUdpEndPoint);
                        break;

                    case NetPackageFlag.CLOSE:
                        outerChannelMap.TryRemove(netId, out _);
                        channel.Close();
                        break;
                }
            }
        }

        TcpServer<GateTcpConnectHander> outerTcpServer;
        UdpServer outerUdpServer;
        UdpServer innerUdpServer;

        readonly ConcurrentDictionary<long, BaseNetChannel> outerChannelMap = new ConcurrentDictionary<long, BaseNetChannel>();
        GateServer() { }

        public async Task Start()
        {
            await ServerList.Instance.Start();
            var setting = Settings.InsAs<GateSettings>();
            innerUdpServer = new UdpServer(setting.InnerUdpPort, OnUdpRecvInner);
            outerUdpServer = new UdpServer(setting.OuterPort, OnUdpRecvOuter);
            outerTcpServer = new();
            _ = innerUdpServer.Start();
            _ = outerUdpServer.Start();
            _ = outerTcpServer.Start(setting.OuterPort);
            _ = CheckTimeOut();
        }

        //检测udp超时
        async Task CheckTimeOut()
        {
            var removeList = new List<long>();

            while (Settings.AppRunning)
            {
                removeList.Clear();
                var timeNow = DateTime.UtcNow;
                //这里暂时只检测udp
                foreach (var kv in outerChannelMap)
                {
                    if (kv.Value is UdpChannel udpChannel)
                    {
                        if (udpChannel.IsClose() || udpChannel.GetLastMessageTimeSecond(timeNow) > 10) //10秒超时
                        {
                            udpChannel.Close();
                            removeList.Add(kv.Key);
                        }
                    }
                }
                foreach (var k in removeList)
                {
                    LOGGER.Info($"超时移除:{k}");
                    outerChannelMap.Remove(k, out _);
                }
                await Task.Delay(1500);
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

            if (outerChannelMap.TryGetValue(netId, out var channel))
            {
                if (channel.IsClose() || channel.TargetServerId != serverId)
                {
                    LOGGER.Warn($"客户端channel关闭或者服务器id不匹配:{netId} {channel.TargetServerId} {serverId}");
                    innerUdpServer.SendTo(new TempNetPackage(NetPackageFlag.NO_GATE_CONNECT, netId, serverId), endPoint);
                    return;
                }
            }
            else
            {
                LOGGER.Warn($"不能发现客户端channel:{package.flag} {netId} {serverId}");
                innerUdpServer.SendTo(new TempNetPackage(NetPackageFlag.NO_GATE_CONNECT, netId, serverId), endPoint);
                return;
            }

            switch (package.flag)
            {
                case NetPackageFlag.ACK: //响应客户端连接
                    {
                        //通知内部服务器 
                        //LOGGER.Info($"向客户端发送消息长度:{package.body.Length}");
                        channel.Write(package);
                        break;
                    }

                case NetPackageFlag.MSG:
                    // LOGGER.Info($"转发内网数据到客户端:{package.body.Length}");
                    channel.Write(package);
                    break;

                case NetPackageFlag.CLOSE:
                    outerChannelMap.Remove(netId, out _);
                    channel.Write(package);
                    channel.Close();
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
                return;
            }

            //LOGGER.Info($"收到外部udp包:{package.ToString()}");

            BaseNetChannel channel = null;
            if (netId != 0)
            {
                outerChannelMap.TryGetValue(netId, out channel);
                if (package.flag == NetPackageFlag.MSG || package.flag == NetPackageFlag.GATE_HEART)
                    if (channel == null || channel is not UdpChannel || channel.IsClose() || channel.TargetServerId != package.innerServerId)
                    {
                        LOGGER.Warn($"当前接收到来自netid:{netId}的udp消息，但channel不是udpchannel或者已经关闭，或者innerServerId不匹配...");
                        package.flag = NetPackageFlag.NO_GATE_CONNECT;
                        package.body = Span<byte>.Empty;
                        outerUdpServer.SendTo(package, endPoint);
                        outerChannelMap.Remove(netId, out _);
                        return;
                    }
            }

            //内网服务器信息
            var server = ServerList.Instance.GetServer(package.innerServerId, ServerType.Game);
            if (server == null)
            {
                package.flag = NetPackageFlag.CLOSE;
                outerChannelMap.Remove(netId, out _);
                outerUdpServer.SendTo(package, endPoint);
                return;
            }

            switch (package.flag)
            {
                case NetPackageFlag.SYN:
                    {
                        //LOGGER.Info($"收到udp连接请求 netid:{package.ToString()}");
                        if (netId == 0)
                        {
                            netId = IdGenerator.GetActorID(ActorType.Gate);
                        }
                        else
                        {
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
                        innerUdpServer.SendTo(package, server.InnerUdpEndPoint);
                        break;
                    }

                case NetPackageFlag.GATE_HEART:
                    //LOGGER.Info($"kcpservice recv GATE_HEART: {netId}");
                    channel.UpdateRecvMessageTime();
                    outerUdpServer.SendTo(package, endPoint);
                    innerUdpServer.SendTo(package, server.InnerUdpEndPoint);
                    break;

                case NetPackageFlag.MSG:
                    channel.UpdateRecvMessageTime();
                    innerUdpServer.SendTo(package, server.InnerUdpEndPoint);
                    break;

                case NetPackageFlag.CLOSE:
                    outerChannelMap.TryRemove(netId, out channel);
                    if (channel is UdpChannel)
                    {
                        channel.Close();
                        innerUdpServer.SendTo(package, server.InnerUdpEndPoint);
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
