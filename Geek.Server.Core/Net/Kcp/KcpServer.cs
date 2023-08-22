using Geek.Server.Core.Actors;
using Geek.Server.Core.Utils;
using NLog;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;

namespace Geek.Server.Core.Net.Kcp
{
    public class KcpServer
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<long, KcpChannel> channels = new();
        private Func<BaseNetChannel, Message, Task> onMessageAct;
        private Action<KcpChannel> onChannelRemove;
        private UdpServer udpServer;

        public KcpServer(int port, Func<BaseNetChannel, Message, Task> onMessageAct, Action<KcpChannel> onChannelRemove)
        {
            LOGGER.Info($"开始kcp server...{port}");
            this.onMessageAct = onMessageAct;
            this.onChannelRemove = onChannelRemove;
            udpServer = new UdpServer(port, OnRecv);
        }

        public Task Start()
        {
            _ = udpServer.Start();
            return Task.Factory.StartNew(UpdateChannel, CancellationToken.None, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            foreach (long channelId in this.channels.Keys.ToArray())
            {
                channels.TryRemove(channelId, out var channel);
                if (channel != null)
                    channel.Close();
            }
            udpServer.Close();
        }

        async Task UpdateChannel(object o)
        {
            List<long> removeList = new List<long>();
            while (true)
            {
                var time = DateTime.UtcNow;
                foreach (var kv in channels)
                {
                    var channel = kv.Value;
                    channel.Update(time);
                    if (channel.IsClose())
                    {
                        removeList.Add(kv.Key);
                    }
                }

                foreach (var id in removeList)
                {
                   // LOGGER.Info($"移除kcpchannel：{id}");
                    channels.Remove(id, out var channel);
                    onChannelRemove?.Invoke(channel);
                }
                removeList.Clear();
                await Task.Delay(10);
            }
        }

        void OnRecv(TempNetPackage package, EndPoint ipEndPoint)
        {
            //LOGGER.Warn($"kcp server 收到包:{package.flag} {package.netId} {package.innerServerId}");
            long netId = package.netId;
            int innerServerNetId = package.innerServerId;

            //检查网络id是否合格 
            if (IdGenerator.GetActorType(netId) != ActorType.Gate)
            {
                LOGGER.Warn($"不正确的gate net id:{netId}");
                return;
            }

            //检查内网服务器id
            if (innerServerNetId == 0 || innerServerNetId != Settings.ServerId)
            {
                LOGGER.Warn($"kcp消息错误,不正确的内网服id:{innerServerNetId}");
                return;
            }

            KcpChannel channel = null;
            channels.TryGetValue(netId, out channel);
            if(channel==null ||channel.IsClose())
            {
                channel = null;
            }else
            { 
                channel.UpdateRecvMessageTime();
            }

            try
            {
                switch (package.flag)
                {
                    case NetPackageFlag.SYN:
                        LOGGER.Info($"kcp server 收到连接请求包:{package.ToString()}");
                        if (channel == null || channel.IsClose())
                        {
                            channel = new KcpChannel(true, netId, Settings.ServerId, ipEndPoint, (chann, data) =>
                            {
                                var tmpPackage = new TempNetPackage(NetPackageFlag.MSG, chann.NetId, chann.TargetServerId, data);
                                //LOGGER.Info($"kcp发送udp数据到gate:{(chann as KcpChannel).routerEndPoint?.ToString()}");
                                udpServer.SendTo(tmpPackage, (chann as KcpChannel).routerEndPoint);
                            }, onMessageAct);
                            channels[channel.NetId] = channel;
                        }
                        //更新最新路由地址
                        var ipep = ipEndPoint as IPEndPoint;
                        channel.routerEndPoint = new IPEndPoint(ipep.Address, ipep.Port);
                        channel.UpdateRecvMessageTime(TimeSpan.FromSeconds(5).Ticks);

                        udpServer.SendTo(new TempNetPackage(NetPackageFlag.ACK, netId, Settings.ServerId), ipEndPoint);
                        break;


                    case NetPackageFlag.MSG:
                        if (channel == null)
                        {
                            LOGGER.Info($"kcpservice recv msg, channel 不存在: {netId}");
                            udpServer.SendTo(new TempNetPackage(NetPackageFlag.CLOSE, netId, Settings.ServerId), ipEndPoint); 
                        }else
                        { 
                            channel.HandleRecv(package.body);
                        }
                        break;

                    case NetPackageFlag.GATE_HEART:
                        if (channel == null )
                        {
                            LOGGER.Info($"kcpservice recv gate_heart, channel 不存在: {netId}");
                            udpServer.SendTo(new TempNetPackage(NetPackageFlag.CLOSE, netId, Settings.ServerId), ipEndPoint);
                        }else
                        {
                            channel.UpdateRecvMessageTime();
                        }
                        break;

                    case NetPackageFlag.NO_GATE_CONNECT: 
                            channel?.UpdateRecvMessageTime(TimeSpan.FromSeconds(-8).Ticks); 
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
