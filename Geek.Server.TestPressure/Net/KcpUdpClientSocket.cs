using Geek.Server.Core.Net.Kcp;
using Geek.Server.Core.Utils;
using Microsoft.AspNetCore.DataProtection;
using NLog;
using NLog.Targets;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Geek.Server.TestPressure.Net
{
    public class KcpUdpClientSocket : IKcpSocket
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        UdpClient socket;
        //IPEndPoint serverEndPoint;
        CancellationTokenSource cancelSrc;
        OnReceiveNetPackFunc onRecv;
        Action onGateClose;
        Action onServerClose;
        public long NetId { get; set; }
        public int ServerId { get; set; }
        public KcpUdpClientSocket(int serverId)
        {
            this.ServerId = serverId;
        }

        public async Task<bool> Connect(string ip, int port, long netId = 0)
        {
            try
            {
                socket = new UdpClient(ip, port);
                //socket.ExclusiveAddressUse = true;
            }
            catch (Exception e)
            {
                LOGGER.Error(e);
                return false;
            }
            this.NetId = netId;
            cancelSrc = new CancellationTokenSource();
            // serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            var data = new byte[13];
            data.Write(NetPackageFlag.SYN, 0);
            data.Write(NetId, 1);
            data.Write(ServerId, 9);

            socket.Send(data);
            try
            {
                var task = socket.ReceiveAsync(cancelSrc.Token).AsTask();
                if (task == await Task.WhenAny(task, Task.Delay(400)))
                {
                    var rcvBuffer = task.Result.Buffer;
                    if (task.Result.Buffer.Length >= 13)
                    {
                        var flag = rcvBuffer[0];
                        NetId = rcvBuffer.ReadLong(1);
                        var serId = rcvBuffer.ReadInt(9);
                        if (flag == NetPackageFlag.ACK)
                        {
                            LOGGER.Info($"连接成功..");
                            return true;
                        }
                        if (flag == NetPackageFlag.NO_GATE_CONNECT && serId == 0)
                        {
                            LOGGER.Error($"内部服务器{ServerId}已关闭或者不存在，连接失败...");
                            Close();
                            return false;
                        }
                    }
                }
                else
                {
                    cancelSrc.Cancel();
                    LOGGER.Info("接收udp消息失败....");
                }
            }
            catch (Exception e)
            {
                LOGGER.Error(e.Message);
                return false;
            }

            Close();
            return false;
        }

        public void Close()
        {
            LOGGER.Error("关闭udp client...");
            cancelSrc?.Cancel();
            socket?.Close();
            cancelSrc = null;
            socket = null;
        }

        public void Send(TempNetPackage package)
        {
            if (socket == null)
                return;
            Span<byte> target = stackalloc byte[package.Length];
            target[0] = package.flag;
            int offset = 1;
            target.Write(package.netId, ref offset);
            target.Write(package.innerServerId, ref offset);
            if (!package.body.IsEmpty)
            {
                package.body.CopyTo(target[13..]);
            }
            socket.Send(target);
        }

        void StartGateHeart()
        { 
            Task.Run(async () =>
            {
                while (!cancelSrc.IsCancellationRequested)
                {
                    try
                    { 
                        Send(new TempNetPackage(NetPackageFlag.GATE_HEART, NetId, ServerId));
                        await Task.Delay(3000, cancelSrc.Token);
                    }
                    catch (Exception e)
                    { 
                        break;
                    }
                }
            });
        }

        public async Task StartRecv(OnReceiveNetPackFunc onRecv, Action onGateClose, Action onServerClose)
        {
            LOGGER.Error("StartRecv...");
            this.onRecv = onRecv;
            this.onGateClose = onGateClose;
            this.onServerClose = onServerClose;
            EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

            void onRecvUdpData(Span<byte> data)
            {
                var package = new TempNetPackage(data);
                if (package.netId != NetId)
                    return;
                switch (package.flag)
                {
                    case NetPackageFlag.NO_GATE_CONNECT:
                        LOGGER.Error("gate 断开连接...");
                        onGateClose?.Invoke();
                        Close();
                        break;
                    case NetPackageFlag.CLOSE:
                        LOGGER.Error("server 断开连接...");
                        onServerClose?.Invoke();
                        Close();
                        break;
                    case NetPackageFlag.MSG:
                        onRecv?.Invoke(package.body);
                        break;
                }
            }

            StartGateHeart();

            await Task.Delay(1);

            while (!cancelSrc.IsCancellationRequested)
            {
                try
                {
                    var result = await socket.ReceiveAsync(cancelSrc.Token);
                    //LOGGER.Warn($"收到udp数据：{result.ReceivedBytes}");
                    var buffer = result.Buffer;
                    if (buffer.Length >= 13)
                    {
                        onRecvUdpData(buffer);
                    }
                }
                catch (Exception e)
                {
                    LOGGER.Warn(e.Message);
                    onGateClose?.Invoke();
                    Close();
                    break;
                }
            }
        }
    }
}
