using Geek.Server.Core.Actors;
using Geek.Server.Core.Utils;
using NLog;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Geek.Server.Core.Net.Kcp
{
    public class KcpServer
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<long, KcpChannel> channels = new();
        private Func<KcpChannel, Message, Task> onMessageAct;
        private Action<KcpChannel> onChannelRemove;
        private UdpServer udpServer;

        public KcpServer(int port, Func<KcpChannel, Message, Task> onMessageAct, Action<KcpChannel> onChannelRemove)
        {
            LOGGER.Info($"开始kcp server...{port}");
            this.onMessageAct = onMessageAct;
            this.onChannelRemove = onChannelRemove;
            udpServer = new UdpServer(port, OnRecv);
        }

        public void Start()
        {
            _ = udpServer.Start();
            _ = UpdateChannel();
        }

        public void Stop()
        {
            foreach (long channelId in channels.Keys.ToArray())
            {
                channels.TryRemove(channelId, out var channel);
                if (channel != null)
                    channel.Close();
            }
            udpServer.Close();
        }

        async Task UpdateChannel()
        {
            List<KcpChannel> channelList = new();
            var paraOpt = new ParallelOptions { MaxDegreeOfParallelism = 3 };
            while (true)
            {
                var time = DateTime.UtcNow;
                channelList.Clear();
                foreach (var kv in channels)
                {
                    channelList.Add(kv.Value);
                }
                Parallel.ForEach(channelList, paraOpt, (channel) =>
                {
                    try
                    {
                        channel.Update(time);
                    }
                    catch (Exception e)
                    {
                        LOGGER.Error("kcp channel update error:" + e.Message);
                    }
                    if (channel.IsClose())
                    {
                        LOGGER.Info($"移除channel...{channel.NetId}");
                        channels.Remove(channel.NetId, out _);
                        onChannelRemove?.Invoke(channel);
                    }
                });
                await Task.Delay(10);
            }
        }

        void OnRecv(TempNetPackage package, EndPoint ipEndPoint)
        {
            //LOGGER.Info($"kcp server 收到包:{package.ToString()}");
            long netId = package.netId;
            int innerServerNetId = package.innerServerId;
            var curServerId = Settings.Ins.ServerId;

            //检查内网服务器id
            if (innerServerNetId == 0 || innerServerNetId != curServerId)
            {
                LOGGER.Warn($"kcp消息错误,不正确的内网服id:{innerServerNetId}");
                return;
            }

            KcpChannel channel = null;
            channels.TryGetValue(netId, out channel);
            if (channel == null || channel.IsClose())
            {
                channel = null;
            }
            else
            {
                channel.UpdateRecvMessageTime();
            }

            try
            {
                switch (package.flag)
                {
                    case NetPackageFlag.SYN:
                        //LOGGER.Info($"kcp server 收到连接请求包:{package.ToString()}");
                        if (channel == null || channel.IsClose())
                        {
                            channel = new KcpChannel(true, netId, curServerId, ipEndPoint, (chann, data) =>
                            {
                                var tmpPackage = new TempNetPackage(NetPackageFlag.MSG, chann.NetId, chann.TargetServerId, data);
                                //LOGGER.Info($"kcp发送udp数据到gate:{(chann as KcpChannel).routerEndPoint?.ToString()}");
                                udpServer.SendTo(tmpPackage, (chann as KcpChannel).routerEndPoint);
                            }, onMessageAct);
                            channels[channel.NetId] = channel;
                            channel.RemoteAddress = Encoding.UTF8.GetString(package.body);
                        }
                        //更新最新路由地址
                        var ipep = ipEndPoint as IPEndPoint;
                        channel.routerEndPoint = new IPEndPoint(ipep.Address, ipep.Port);
                        channel.UpdateRecvMessageTime(TimeSpan.FromSeconds(5).Ticks);

                        udpServer.SendTo(new TempNetPackage(NetPackageFlag.ACK, netId, curServerId), ipEndPoint);
                        break;

                    case NetPackageFlag.SYN_OLD_NET_ID:
                        if (channel == null || channel.IsClose())
                        {
                            udpServer.SendTo(new TempNetPackage(NetPackageFlag.CLOSE, netId, curServerId), ipEndPoint);
                        }
                        else
                        {
                            udpServer.SendTo(new TempNetPackage(NetPackageFlag.ACK, netId, curServerId), ipEndPoint);
                            channel.UpdateRecvMessageTime(TimeSpan.FromSeconds(5).Ticks);
                        }
                        break;

                    case NetPackageFlag.MSG:
                        if (channel == null)
                        {
                            LOGGER.Info($"kcpservice recv msg, channel 不存在: {netId}");
                            udpServer.SendTo(new TempNetPackage(NetPackageFlag.CLOSE, netId, curServerId), ipEndPoint);
                        }
                        else
                        {
                            channel.HandleRecv(package.body);
                        }
                        break;

                    case NetPackageFlag.HEART:
                        if (channel == null || channel.IsClose())
                        {
                            LOGGER.Info($"kcpservice recv gate_heart, channel 不存在: {netId}");
                            udpServer.SendTo(new TempNetPackage(NetPackageFlag.CLOSE, netId, curServerId), ipEndPoint);
                        }
                        else
                        {
                            channel.UpdateRecvMessageTime();
                        }
                        break;

                    case NetPackageFlag.NO_GATE_CONNECT:
                        channel?.UpdateRecvMessageTime(TimeSpan.FromSeconds(-5).Ticks);
                        break;

                    case NetPackageFlag.CLOSE:
                        channel.Close();
                        break;
                }
            }
            catch (Exception e)
            {
                LOGGER.Error($"kcpservice error:{e}");
            }
        }
    }
}
