
using Base;
using MessagePack;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class KcpTcpClientSocket : AKcpSocket
{
    public delegate void ReceiveFunc(TempNetPackage package);
    //ConnectionContext context;
    private TcpClient socket;
    Pipe dataPipe;

    public KcpTcpClientSocket(int serverId)
    {
        this.ServerId = serverId;
    }

    public override async Task<ConnectResult> Connect(string ip, int port, long netId = 0)
    {
        Debug.Log($"开始连接:{ip} {port}");
        isConnecting = true;
        this.NetId = netId;
        //context = await new SocketConnection(AddressFamily.InterNetwork, ip, port).StartAsync();
        socket = new TcpClient(AddressFamily.InterNetwork);
        if (socket == null)
            return new(false, true, false);

        try
        {
            var task = socket.ConnectAsync(ip, port);
            var tokenSource = new CancellationTokenSource();
            var completeTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5), tokenSource.Token));
            if (completeTask != task)
            {
                Close();
                return new(false, true, false);
            }
            else
            {
                tokenSource.Cancel();
                await task;
            }
        }
        catch (Exception e)
        {
            Close();
            return new(false, true, false);
        }

        dataPipe = new Pipe();
        _ = readSocketData();

        Send(new TempNetPackage(NetPackageFlag.SYN, NetId, ServerId));

        ConnectResult retResult = new()
        {
            allowReconnect = true
        };

        //等待连接消息
        if (!cancelSrc.IsCancellationRequested)
        {
            var cancelToken = cancelSrc.Token;
            while (!cancelSrc.IsCancellationRequested)
            {
                try
                {
                    var result = await dataPipe.Reader.ReadAsync(cancelToken);
                    var buf = result.Buffer;
                    if (TryParseNetPack(ref buf, (pack) =>
                    {
                        NetId = pack.netId;
                        try
                        {
                            var dic = MessagePackSerializer.Deserialize<Dictionary<string, string>>(pack.body.ToArray());
                            retResult.newGateIp = dic["ip"];
                            retResult.newGatePort = int.Parse(dic["port"]);
                        }
                        catch { }
                        if (pack.flag == NetPackageFlag.ACK)
                        {
                            Debug.Log($"连接成功..");
                            retResult.isSuccess = true;
                        }
                        if (pack.flag == NetPackageFlag.NO_GATE_CONNECT)
                        {
                        }
                        if (pack.flag == NetPackageFlag.NO_INNER_SERVER) //不能发现服务器
                        {
                            retResult.allowReconnect = false;
                            retResult.resetNetId = true;
                        }
                        if (pack.flag == NetPackageFlag.CLOSE) //服务器已关闭连接
                        {
                            retResult.resetNetId = true;
                        }
                    }))
                    {
                        dataPipe.Reader.AdvanceTo(buf.Start, buf.End);
                        break;
                    }
                    else
                    {
                        dataPipe.Reader.AdvanceTo(buf.Start, buf.End);
                    }
                }
                catch
                {
                }
            }
        }
        isConnecting = false;
        return retResult;
    }

    async Task readSocketData()
    {
        byte[] readBuffer = new byte[1024 * 20];
        var dataPipeWriter = dataPipe.Writer;
        var cancelToken = cancelSrc.Token;
        while (!cancelSrc.IsCancellationRequested && !IsClose())
        {
            try
            {
                var length = await socket.GetStream().ReadAsync(readBuffer, 0, readBuffer.Length, cancelToken);
                if (length > 0)
                {
                    //Debug.Log($"收到网络包：{length}");
                    dataPipeWriter.Write(readBuffer.AsSpan()[..length]);
                    var flushTask = dataPipeWriter.FlushAsync();
                    if (!flushTask.IsCompleted)
                    {
                        await flushTask.ConfigureAwait(false);
                    }
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        }
        Close();
    }

    async Task ReadPackAsync(ReceiveFunc onPack)
    {
        var readCancelToken = cancelSrc.Token;
        while (!cancelSrc.IsCancellationRequested)
        {
            try
            {
                var result = await dataPipe.Reader.ReadAsync(readCancelToken);
                var buf = result.Buffer;
                while (!cancelSrc.IsCancellationRequested && TryParseNetPack(ref buf, onPack))
                {

                }
                dataPipe.Reader.AdvanceTo(buf.Start, buf.End);
            }
            catch
            {
                break;
            }
        }
        Debug.Log($"ReadPackAsync exit.....");
    }

    bool TryParseNetPack(ref ReadOnlySequence<byte> input, ReceiveFunc onPack)
    {
        var netReader = dataPipe.Reader;
        var reader = new MessagePack.SequenceReader<byte>(input);

        if (!reader.TryReadBigEndian(out int msgLen))
        {
            return false;
        }

        if (msgLen < TempNetPackage.headLen)
        {
            throw new Exception($"从客户端接收的包大小异常,{msgLen} {reader.Remaining}:至少大于8个字节");
        }
        else if (msgLen > 1500)
        {
            throw new Exception("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值1500字节");
        }

        if (reader.Remaining < msgLen)
        {
            return false;
        }

        reader.TryRead(out byte flag);
        reader.TryReadBigEndian(out long netId);
        reader.TryReadBigEndian(out int serverId);
        var dataLen = msgLen - TempNetPackage.headLen;

        var payload = input.Slice(reader.Position, dataLen);
        Span<byte> data = stackalloc byte[dataLen];
        payload.CopyTo(data);
        onPack(new TempNetPackage(flag, netId, serverId, data));

        input = input.Slice(input.GetPosition(msgLen + 4));
        return true;
    }

    public override bool IsClose()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return true;
        }
#endif
        if (isConnecting)
            return false;
        if (socket == null)
        {
            return true;
        }
        if (!socket.Connected)
            return true;
        try
        {
            if (socket.Client.Poll(1000, SelectMode.SelectRead) && socket.Client.Available == 0)
            {
                Debug.LogError("Poll.... ................end");
                return true;
            }
        }
        catch
        {
            return true;
        }

        return false;
    }

    public override void Close()
    {
        lock (this)
        {
            try
            {
                if (socket != null)
                {
                    base.Close();
                    socket.Close();
                    socket.Dispose();
                }
            }
            catch { }
            finally
            {
                dataPipe = null;
                socket = null;
            }
        }
    }

    public override void Send(TempNetPackage package)
    {
        Span<byte> target = stackalloc byte[package.Length + 4];
        int offset = 0;
        target.Write(package.Length, ref offset);
        target.Write(package, ref offset);
        //netWriter.Write(target);
        Debug.Log($"写tcp pack:{package.ToString()}");
        //netWriter.FlushAsync();

        if (IsClose())
            return;
        socket.GetStream().Write(target);
    }

    public override async Task StartRecv(OnReceiveNetPackFunc onRecv, Action onGateClose, Action onServerClose)
    {
        _ = StartGateHeartAsync();
        await ReadPackAsync((package) =>
        {
#if UNITY_EDITOR
            //Debuger.Log($"收到包...{package.ToString()}");
#endif
            if (package.netId != NetId)
                return;
            switch (package.flag)
            {
                case NetPackageFlag.NO_GATE_CONNECT:
                    Debug.LogError("gate 断开连接...");
                    Close();
                    onGateClose?.Invoke();
                    onGateClose = null;
                    break;
                case NetPackageFlag.CLOSE:
                case NetPackageFlag.NO_INNER_SERVER:
                    Debug.LogError("server 断开连接...");
                    Close();
                    onServerClose?.Invoke();
                    break;
                case NetPackageFlag.HEART:
                    if (package.body.Length > 0)
                    {
                        var id = BinaryPrimitives.ReadInt32BigEndian(package.body);
#if UNITY_EDITOR
                        Debug.Log($"收到心跳回复包...{id}");
#endif 
                        EndWaitHeartId(id);
                    }
                    break;
                case NetPackageFlag.MSG:
                    onRecv?.Invoke(package.body);
                    break;
            }
        });
        //网络连接主动断开，则认为是网关断开
        Debug.Log("tcp StartRecv end....");
        onGateClose?.Invoke();
    }
}