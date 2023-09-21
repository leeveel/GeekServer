
using MessagePack;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

public class NetChannel
{
    protected Action<Message> onMessage;
    protected Action onClose;
    protected Pipe recvPipe;
    protected TcpClient socket;
    protected CancellationTokenSource closeSrc = new CancellationTokenSource();

    public NetChannel(TcpClient socket, Action<Message> onMessage = null, Action onClose = null)
    {
        this.socket = socket;
        this.onMessage = onMessage;
        this.onClose = onClose;
        recvPipe = new Pipe();
    }

    public virtual void Close()
    {
        closeSrc.Cancel();
    }

    public virtual bool IsClose()
    {
        return closeSrc.IsCancellationRequested;
    }

    public async Task StartAsync()
    {
        try
        {
            _ = recvNetData();
            var cancelToken = closeSrc.Token;
            while (!cancelToken.IsCancellationRequested)
            {
                var result = await recvPipe.Reader.ReadAsync(cancelToken);
                var buffer = result.Buffer;
                if (buffer.Length > 0)
                {
                    SequencePosition examined = buffer.Start;
                    SequencePosition consumed = examined;
                    TryParseMessage(buffer, ref consumed, ref examined);
                    recvPipe.Reader.AdvanceTo(consumed, examined);
                }
                else if (result.IsCanceled || result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        Close();
        onClose?.Invoke();
    }

    async Task recvNetData()
    {
        byte[] readBuffer = new byte[2048];
        var dataPipeWriter = recvPipe.Writer;
        var cancelToken = closeSrc.Token;
        while (!cancelToken.IsCancellationRequested)
        {
            var length = await socket.GetStream().ReadAsync(readBuffer, 0, readBuffer.Length, cancelToken);
            if (length > 0)
            {
                dataPipeWriter.Write(readBuffer.AsSpan().Slice(0, length));
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
    }

    protected virtual void TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined)
    {
        var reader = new SequenceReader<byte>(input);

        if (!reader.TryReadBigEndian(out int length) || reader.Remaining < length - 4)
        {
            consumed = input.End; //告诉read task，到这里为止还不满足一个消息的长度，继续等待更多数据
            return;
        }

        var payload = input.Slice(reader.Position, length - 4);
        if (payload.Length < 4)
            throw new Exception("消息长度不够");

        consumed = payload.End;
        examined = consumed;

        //消息id
        reader.TryReadBigEndian(out int msgId);

        var message = MessagePackSerializer.Deserialize<Message>(payload.Slice(4));
        Debug.Log("收到消息:" + MessagePackSerializer.SerializeToJson(message));
        if (message.MsgId != msgId)
        {
            throw new Exception($"解析消息错误，注册消息id和消息无法对应.real:{message.MsgId}, register:{msgId}");
        }
        onMessage(message);
        return;
    }

    private const int Magic = 0x1234;
    int count = 0;
    public void Write(Message msg)
    {
        if (IsClose())
            return;
        var bytes = MessagePackSerializer.Serialize(msg);
        int len = 4 + 8 + 4 + 4 + bytes.Length;
        var buffer = ArrayPool<byte>.Shared.Rent(len);

        int magic = Magic + ++count;
        magic ^= Magic << 8;
        magic ^= len;

        int offset = 0;
        var buffSpan = buffer.AsSpan();
        buffSpan.WriteInt(len, ref offset);
        buffSpan.WriteLong(DateTime.Now.Ticks, ref offset);
        buffSpan.WriteInt(magic, ref offset);
        buffSpan.WriteInt(msg.MsgId, ref offset);
        buffSpan.WriteBytesWithoutLength(bytes, ref offset);
        socket.GetStream().Write(buffer, 0, len);
        ArrayPool<byte>.Shared.Return(buffer);
    }
}