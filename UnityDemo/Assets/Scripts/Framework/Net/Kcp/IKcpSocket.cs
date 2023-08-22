
using System;
using System.Threading.Tasks;

public delegate void OnReceiveNetPackFunc(ReadOnlySpan<byte> data);

public static class NetPackageFlag
{
    public const byte SYN = 56;          //client->gate->innerServer
    public const byte ACK = 31;          //innerServer->gate->client
    public const byte GATE_HEART = 39;  //client->gate->innerServer
    public const byte NO_GATE_CONNECT = 99;  //client->gate
    public const byte CLOSE = 68;
    public const byte MSG = 111;        //client->gate->innerserver innerserver->gate->client 
}

public ref struct TempNetPackage
{
    public bool isOk;
    public byte flag;
    public long netId;
    public int innerServerId;  //当返回fin标志时  如果此id是serverid 表示网关断开，如果是0 标识不能发现内部服务器  如果是负数 标识内部服务器主动断开
    public ReadOnlySpan<byte> body;

    public TempNetPackage(byte flag, long netId, int targetServerId = 0)
    {
        isOk = true;
        this.flag = flag;
        this.netId = netId;
        innerServerId = targetServerId;
        body = Span<byte>.Empty;
    }

    public TempNetPackage(byte flag, long netId, int targetServerId, ReadOnlySpan<byte> data)
    {
        isOk = true;
        this.flag = flag;
        this.netId = netId;
        innerServerId = targetServerId;
        body = data;
    }

    public TempNetPackage(Span<byte> data)
    {
        if (data.Length < 13)
        {
            isOk = false;
            flag = 0;
            netId = 0;
            innerServerId = 0;
            body = data;
            return;
        }
        isOk = true;
        flag = data[0];
        netId = data.ReadLong(1);
        innerServerId = data.ReadInt(9);
        body = data.Slice(13);
    }

    public int Length
    {
        get => 13 + body.Length;
    }

    public override string ToString()
    {
        return $"flag:{flag} netId:{netId} innerServerId:{innerServerId} bodyLen:{body.Length}";
    }
}

public interface IKcpSocket
{
    Task<bool> Connect(string ip, int port, long netId = 0);
    long NetId { get; set; }
    int ServerId { get; set; }
    void Send(TempNetPackage data);
    void Close();
    Task StartRecv(OnReceiveNetPackFunc onRecv, Action onGateClose, Action onServerClose);
}
