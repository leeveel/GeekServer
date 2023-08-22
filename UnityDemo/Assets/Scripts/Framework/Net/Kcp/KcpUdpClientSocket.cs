
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class KcpUdpClientSocket : IKcpSocket
{
    UdpClient socket;
    //IPEndPoint serverEndPoint;
    CancellationTokenSource cancelSrc = new CancellationTokenSource();
    OnReceiveNetPackFunc onRecv;
    Action onGateClose;
    Action onServerClose;
    public long NetId { get; set; }
    public int ServerId { get; set; }
    public KcpUdpClientSocket(int serverId)
    {
        this.ServerId = serverId;
        //socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
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
            Debug.LogError(e);
            return false;
        }
        this.NetId = netId;
        cancelSrc = new CancellationTokenSource();
        //serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        var data = new byte[13];
        data.Write(NetPackageFlag.SYN, 0);
        data.Write(NetId, 1);
        data.Write(ServerId, 9);
        //Debug.Log($"开始udp连接....{NetId}");
        //连续发多条消息，如果没收到 判定连接失败
        EndPoint msgIpEnd = new IPEndPoint(IPAddress.Any, 0);

        socket.Send(data, data.Length);
        try
        {
            var task = socket.ReceiveAsync();
            if (task == await Task.WhenAny(task, Task.Delay(400)))
            {
                var buffer = task.Result.Buffer;
                if (buffer.Length >= 13)
                {
                    var flag = buffer[0];
                    NetId = buffer.ReadLong(1);
                    var serId = buffer.ReadInt(9);
                    if (flag == NetPackageFlag.ACK)
                    {
                        Debug.Log($"连接成功..");
                        return true;
                    }
                    if (flag == NetPackageFlag.NO_GATE_CONNECT && serId == 0)
                    {
                        Debug.LogError($"内部服务器{ServerId}已关闭或者不存在，连接失败...");
                        Close();
                        return false;
                    }
                }
            }
            else
            {
                cancelSrc.Cancel();
                Debug.Log("接收udp消息失败....");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }

        Close();
        return false;
    }

    public void Close()
    {
        if (socket == null)
            return;
        cancelSrc.Cancel();
        socket?.Close();
        socket = null;
    }

    byte[] sendBuffer = new byte[2000];
    public void Send(TempNetPackage package)
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
            package.body.CopyTo(target.Slice(13));
        }
        socket.Send(sendBuffer, target.Length);
    }

    void StartGateHeart()
    {
        Task.Run(async () =>
        {
            while (!cancelSrc.IsCancellationRequested)
            {
                Send(new TempNetPackage(NetPackageFlag.GATE_HEART, NetId, ServerId));
                await Task.Delay(2000, cancelSrc.Token);
            }
        });
    }

    public async Task StartRecv(OnReceiveNetPackFunc onRecv, Action onGateCloseAct, Action onServerCloseAct)
    {
        this.onRecv = onRecv;
        this.onGateClose = onGateCloseAct;
        this.onServerClose = onServerCloseAct;

        void onRecvUdpData(byte[] data)
        {
            var package = new TempNetPackage(data);
            if (package.netId != NetId)
                return;
            switch (package.flag)
            {
                case NetPackageFlag.NO_GATE_CONNECT:
                    Debug.LogError("gate 断开连接...");
                    onGateClose?.Invoke();
                    Close();
                    break;
                case NetPackageFlag.CLOSE:
                    Debug.LogError("server 断开连接...");
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
                var result = await socket.ReceiveAsync();
                //LOGGER.Warn($"收到udp数据：{result.ReceivedBytes}");
                var buffer = result.Buffer;
                if (buffer.Length >= 13)
                {
                    onRecvUdpData(buffer);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                onGateClose?.Invoke();
                Close();
                break;
            }
        }
    }
}