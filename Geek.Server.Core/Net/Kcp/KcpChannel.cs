using Bedrock.Framework;
using Geek.Server.Core.Serialize;
using Geek.Server.Core.Utils;
using MessagePack;
using NLog;
using SharpCompress.Writers;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets.Kcp;

namespace Geek.Server.Core.Net.Kcp
{
    public class KcpChannel : BaseNetChannel, IKcpCallback
    {
        public delegate void KcpOutPutFunc(BaseNetChannel chann, ReadOnlySpan<byte> data);
        private readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public EndPoint routerEndPoint { get; set; }

        private KcpOutPutFunc kcpDataSendFunc;
        private Func<BaseNetChannel, Message, Task> onMessageAct;
        private Action onConnectCloseAct;
        private Action onClose;
        private SimpleSegManager.Kcp kcp;
        private bool isColose = false;
        private Pipe dataPipe;

        const int MAX_RECV_SIZE = 1024 * 1024 * 20;
        object writeLockObj = new object();

        public KcpChannel(bool isInner, long id, int serverId, EndPoint routerEndPoint, KcpOutPutFunc kcpSocketSendAct, Func<BaseNetChannel, Message, Task> onMessageAct, Action onClose = null)
        {
            NetId = id;
            var ipep = routerEndPoint as IPEndPoint;
            this.routerEndPoint = new IPEndPoint(ipep.Address, ipep.Port);
            kcp = new SimpleSegManager.Kcp((uint)id, this);
            kcp.NoDelay(1, 10, 2, 1);
            kcp.WndSize(128, 128);
            kcp.SetMtu(isInner ? 1400 : 520);
            dataPipe = new Pipe(PipeOptions.Default);
            kcpDataSendFunc = kcpSocketSendAct;
            this.onMessageAct = onMessageAct;
            this.TargetServerId = serverId;
            this.onClose = onClose;
            UpdateRecvMessageTime(TimeSpan.FromSeconds(5).Ticks);
            _ = StartProcessMsgAsync();
        }


        bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out Message message)
        {
            message = default;
            var reader = new SequenceReader<byte>(input);

            if (!reader.TryReadBigEndian(out int msgLen))
            {
                examined = input.End; //告诉read task，到这里为止还不满足一个消息的长度，继续等待更多数据
                return false;
            }

            if (msgLen <= 8)//(消息长度已经被读取)
            {
                throw new ProtocalParseErrorException($"从客户端接收的包大小异常,{msgLen} {reader.Remaining}:至少大于8个字节");
            }
            else if (msgLen > MAX_RECV_SIZE)
            {
                throw new ProtocalParseErrorException("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值：" + MAX_RECV_SIZE / 1024 + "字节");
            }

            //验证token...

            if (reader.Remaining < msgLen - 4)
            {
                examined = input.End;
                return false;
            }

            reader.TryReadBigEndian(out int msgId);  //4  

            var payload = input.Slice(reader.Position, msgLen - 8);
            message = MessagePackSerializer.Deserialize<Message>(payload);

            consumed = examined = payload.End;
            return true;
        }


        async Task StartProcessMsgAsync()
        {
            while (!isColose)
            {
                try
                {
                    var reader = dataPipe.Reader;
                    var result = await reader.ReadAsync();
                    var buffer = result.Buffer;

                    if (buffer.Length > 0)
                    {
                        SequencePosition examined = buffer.Start;
                        SequencePosition consumed = examined;
                        TryParseMessage(buffer, ref consumed, ref examined, out var msg);
                        reader.AdvanceTo(consumed, examined);
                        if (msg != null)
                        {
                            LOGGER.Info($"收到消息:{msg.GetType().Name}:{MessagePack.MessagePackSerializer.SerializeToJson(msg)}");
                            if (onMessageAct != null)
                                await onMessageAct(this, msg);
                        }
                    }
                    else if (result.IsCanceled || result.IsCompleted)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    LOGGER.Error(e.Message);
                    break;
                }
            }
        }

        public override void Close()
        {
            //LOGGER.Warn($"kcpchannel close:{NetId}"); 
            isColose = true;
            onClose?.Invoke();
            onClose = null;
        }

        public override bool IsClose()
        {
            return isColose;
        }

        public override void Write([NotNull] Message msg)
        {
            if (isColose)
                return;
            LOGGER.Info($"发送{msg.GetType()}:{MessagePack.MessagePackSerializer.SerializeToJson(msg)}");
            var bytes = Serializer.Serialize(msg);

            var headSize = 8;
            Span<byte> span = stackalloc byte[headSize + bytes.Length];
            int len = headSize + bytes.Length;
            int offset = 0;
            span.Write(len, ref offset);
            span.Write(msg.MsgId, ref offset);
            bytes.CopyTo(span.Slice(headSize));
            lock (writeLockObj)
            {
                kcp?.Send(span);
            }
        }

        public void HandleRecv(ReadOnlySpan<byte> data)
        {
            if (isColose)
            {
                return;
            }
            UpdateRecvMessageTime();
            //LOGGER.Error($"kcp intput...");
            kcp.Input(data);
        }

        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            //LOGGER.Error($"kcp output:{avalidLength}");
            kcpDataSendFunc(this, buffer.Memory.Span[..avalidLength]);
            buffer.Dispose();
        }

        public void Update(in DateTime time)
        {
            if (isColose)
                return;
            var lastMsgTime = GetLastMessageTimeSecond(time);
            if (lastMsgTime > 60_0)  //10分钟没数据则关闭  kcp需要保留一段时间channel，客户端重连后如果老channel还在，会尝试继续通信
            {
                Close();
                return;
            }

            if (lastMsgTime > 10)
            {
                return;
            }
            var (buffer, avalidLength) = kcp.TryRecv();
            if (buffer != null && avalidLength > 0)
            {
                UpdateRecvMessageTime();
                var writer = dataPipe.Writer;
                writer.Write(buffer.Memory.Span[..avalidLength]);
                writer.FlushAsync();
            }
            kcp.Update(time);
        }
    }
}
