using Geek.Server.Core.Net.Kcp;
using Geek.Server.Core.Utils;
using System.Buffers.Binary;
using System.Net.Sockets;

namespace Geek.Server.TestPressure.Net
{
    public class KcpUdpClientSocket : AKcpSocket
    {
        UdpClient socket;
        Action onGateClose;
        Action onServerClose;
        public KcpUdpClientSocket(int serverId)
        {
            ServerId = serverId;
        }

        public override async Task<ConnectResult> Connect(string ip, int port, long netId = 0)
        {
            isConnecting = true;
            try
            {
                socket = new UdpClient(ip, port);
                //socket.ExclusiveAddressUse = true;
            }
            catch (Exception e)
            {
                LOGGER.Error(e);
                return new(false, true, false);
            }
            NetId = netId;
            //serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            var data = new byte[TempNetPackage.headLen];
            data.Write(NetPackageFlag.SYN, 0);
            data.Write(NetId, 1);
            data.Write(ServerId, 9);
            //Debug.Log($"开始udp连接....{NetId}");  
            socket.Send(data, data.Length);
            try
            {
                var task = socket.ReceiveAsync();
                if (task == await Task.WhenAny(task, Task.Delay(400)))
                {
                    var buffer = task.Result.Buffer;
                    if (buffer.Length >= TempNetPackage.headLen)
                    {
                        var flag = buffer[0];
                        NetId = buffer.ReadLong(1);
                        var serId = buffer.ReadInt(9);
                        //LOGGER.Info($"收到连接包:{flag}");
                        if (flag == NetPackageFlag.ACK)
                        {
                            return new(true, true, false);
                        }
                        if (flag == NetPackageFlag.NO_GATE_CONNECT)
                        {
                            Close();
                            return new(false, true, false);
                        }
                        if (flag == NetPackageFlag.NO_INNER_SERVER) //不能发现服务器
                        {
                            Close();
                            return new(false, false, true);
                        }
                        if (flag == NetPackageFlag.CLOSE) //服务器已关闭连接
                        {
                            return new(false, true, true);
                        }
                    }
                }
                else
                {
                    Close();
                    LOGGER.Error("接收udp消息失败....");
                }
            }
            catch (Exception e)
            {
                LOGGER.Error(e.Message);
                return new(false, true, false);
            }
            finally
            {
                isConnecting = false;
            }
            Close();
            return new(false, true, false);
        }

        public override void Close()
        {
            lock (this)
            {
                base.Close();
                cancelSrc.Cancel();
                socket?.Close();
                socket = null;
            }
        }

        public override bool IsClose()
        {
            return !isConnecting && socket == null;
        }

        readonly byte[] sendBuffer = new byte[2000];
        public override void Send(TempNetPackage package)
        {
            if (socket == null)
                return;
            var target = new Span<byte>(sendBuffer);
            target[0] = package.flag;
            int offset = 1;
            target.Write(package.netId, ref offset);
            target.Write(package.innerServerId, ref offset);
            if (!package.body.IsEmpty)
            {
                package.body.CopyTo(target.Slice(TempNetPackage.headLen));
            }
            socket.Send(sendBuffer, package.Length);
        }

        public override async Task StartRecv(OnReceiveNetPackFunc onRecv, Action onGateCloseAct, Action onServerCloseAct)
        {
            onGateClose = onGateCloseAct;
            onServerClose = onServerCloseAct;

            _ = StartGateHeartAsync();

            void onRecvUdpData(byte[] data)
            {
                var package = new TempNetPackage(data);
                if (package.netId != NetId)
                    return;
                switch (package.flag)
                {
                    case NetPackageFlag.NO_GATE_CONNECT:
                        onGateClose?.Invoke();
                        Close();
                        break;
                    case NetPackageFlag.CLOSE:
                    case NetPackageFlag.NO_INNER_SERVER:
                        onServerClose?.Invoke();
                        Close();
                        break;
                    case NetPackageFlag.HEART:
                        if (package.body.Length > 0)
                        {
                            var id = BinaryPrimitives.ReadInt32BigEndian(package.body);
                            EndWaitHeartId(id);
                        }
                        break;
                    case NetPackageFlag.MSG:
                        onRecv?.Invoke(package.body);
                        break;
                }
            }

            await Task.Delay(1);

            while (!cancelSrc.IsCancellationRequested)
            {
                try
                {
                    var result = await socket.ReceiveAsync();
                    // Debuger.Log($"收到udp数据：{result.Buffer.Length}");
                    var buffer = result.Buffer;
                    if (buffer.Length >= TempNetPackage.headLen)
                    {
                        onRecvUdpData(buffer);
                    }
                }
                catch (Exception)
                {
                    Close();
                    onGateClose?.Invoke();
                    break;
                }
            }
        }
    }

}
