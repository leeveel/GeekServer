
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public delegate void OnReceiveNetPackFunc(ReadOnlySpan<byte> data);

public static class NetPackageFlag
{
    public const byte SYN = 1;
    public const byte ACK = 3;
    public const byte HEART = 4;
    public const byte NO_GATE_CONNECT = 5;
    public const byte CLOSE = 6;
    public const byte NO_INNER_SERVER = 7;
    public const byte MSG = 8;

    public static string GetFlagDesc(byte flag)
    {
        return flag switch
        {
            SYN => "连接请求",
            ACK => "连接应答",
            HEART => "网关心跳",
            NO_GATE_CONNECT => "无网关连接",
            CLOSE => "关闭",
            MSG => "消息",
            NO_INNER_SERVER => "无内部服务器",
            _ => "无效标记:" + flag,
        };
    }
}

public ref struct TempNetPackage
{
    public const int headLen = 13;
    public bool isOk;
    public byte flag;
    public long netId;
    public int innerServerId;
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
        if (data.Length < headLen)
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
        body = data[headLen..];
    }

    public readonly int Length => headLen + body.Length;

    public override string ToString()
    {
        return $"flag:{NetPackageFlag.GetFlagDesc(flag)} netId:{netId} innerServerId:{innerServerId} bodyLen:{body.Length}";
    }
}

public class ConnectResult
{
    public bool isSuccess;
    public bool allowReconnect;
    public bool resetNetId;
    public string newGateIp;
    public int newGatePort;

    public ConnectResult()
    {

    }
    public ConnectResult(bool success, bool allowRecon, bool resetNid)
    {
        isSuccess = success;
        allowReconnect = allowRecon;
        resetNetId = resetNid;
    }
}

public abstract class AKcpSocket
{
    public virtual Task<ConnectResult> Connect(string ip, int port, long netId = 0) => throw new NotImplementedException();
    public long NetId { get; set; }
    public int ServerId { get; set; }
    protected bool isConnecting = true;
    protected int immediatelyHeartId = int.MinValue;
    protected CancellationTokenSource cancelSrc = new();

    protected void EndWaitHeartId(int id)
    {
        SyncContextUtil.RunOnUnityScheduler(() =>
        {
            MsgWaiter.EndWait(id, true);
        });
    }

    public virtual void Send(TempNetPackage data) { }
    public virtual void Close()
    {
        EndWaitHeartId(immediatelyHeartId);
        cancelSrc?.Cancel();
    }
    public virtual bool IsClose() { return false; }
    public virtual Task StartRecv(OnReceiveNetPackFunc onRecv, Action onGateClose, Action onServerClose) => Task.CompletedTask;
    public virtual async ValueTask<bool> HeartCheckImmediate()
    {
        if (isConnecting)
            return true;

        if (IsClose())
            return true;

        EndWaitHeartId(immediatelyHeartId);

        immediatelyHeartId = immediatelyHeartId < -100 ? immediatelyHeartId + 1 : int.MinValue;
        var data = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(data, immediatelyHeartId);

        Debug.Log($"立即发送心跳检测包...{immediatelyHeartId}");

        Send(new TempNetPackage(NetPackageFlag.HEART, NetId, ServerId, data));
        return await MsgWaiter.StartWait(immediatelyHeartId, 5, "网络心跳"); //等待返回，超时3秒
    }

    protected async Task StartGateHeartAsync()
    {
        while (!cancelSrc.IsCancellationRequested)
        {
            try
            {
                Send(new TempNetPackage(NetPackageFlag.HEART, NetId, ServerId));
                await Task.Delay(TimeSpan.FromSeconds(3), cancelSrc.Token);
            }
            catch
            {
                break;
            }
        }
    }
}
