using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Serialize;
using MessagePack;
using Microsoft.AspNetCore.Connections;
using SharpCompress.Writers;
using System.Buffers;
using System.IO.Pipelines;

namespace Geek.Server.Core.Net.Tcp
{
    public class TcpChannel : NetChannel
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public ConnectionContext Context { get; protected set; }
        protected PipeReader Reader { get; set; }
        protected PipeWriter Writer { get; set; }


        private readonly SemaphoreSlim sendSemaphore = new(0);

        protected Func<Message, Task> onMessage;

        protected long lastReviceTime = 0;
        protected int lastOrder = 0;
        const int MAX_RECV_SIZE = 1024 * 1024 * 5; /// 从客户端接收的包大小最大值（单位：字节 5M）

        public TcpChannel(ConnectionContext context, Func<Message, Task> onMessage = null)
        {
            this.onMessage = onMessage;
            Context = context;
            Reader = context.Transport.Input;
            Writer = context.Transport.Output;
            RemoteAddress = context.RemoteEndPoint?.ToString();
        }

        public override async Task StartAsync()
        {
            Task reading = ReadAsync();
            Task writing = SendAsync();
            await Task.WhenAll(reading, writing);
            Close();
        }

        async Task ReadAsync()
        {
            try
            {
                var token = Context.ConnectionClosed;
                while (!token.IsCancellationRequested)
                {

                    var result = await Reader.ReadAsync(token);
                    var buffer = result.Buffer;
                    try
                    {
                        if (result.IsCanceled)
                            break;

                        int count = 0;
                        while (TryParseMessage(ref buffer, out var msg))
                        {
                            await onMessage?.Invoke(msg);
                            if (++count > 20)
                            {
                                await Task.Yield();
                                count = 0;
                            }
                        };
                        if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        LOGGER.Error(e);
                        break;
                    }
                    finally
                    {
                        Reader.AdvanceTo(buffer.Start, buffer.End);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                await Reader.CompleteAsync();
            }
        }

        async Task SendAsync()
        {
            var token = Context.ConnectionClosed;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await sendSemaphore.WaitAsync(token);
                        var flush = await Writer.FlushAsync(token);
                        if (flush.IsCompleted || flush.IsCanceled)
                            break;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                await Writer.CompleteAsync();
            }
        }

        protected virtual bool TryParseMessage(ref ReadOnlySequence<byte> input, out Message msg)
        {
            msg = default;
            var bufEnd = input.End;
            var reader = new SequenceReader<byte>(input);

            if (!reader.TryReadBigEndian(out int msgLen))
            {
                return false;
            }

            if (!CheckMsgLen(msgLen))
            {
                throw new Exception("消息长度异常");
            }

            if (reader.Remaining < msgLen - 4)
            {
                return false;
            }

            var payload = input.Slice(reader.Position, msgLen - 4);


            reader.TryReadBigEndian(out long time);
            if (!CheckTime(time))
            {
                throw new Exception("消息接收时间错乱");
            }

            reader.TryReadBigEndian(out int order);
            if (!CheckMagicNumber(order, msgLen))
            {
                throw new Exception("消息order错乱");
            }

            reader.TryReadBigEndian(out int msgId);

            var msgType = HotfixMgr.GetMsgType(msgId);
            if (msgType == null)
            {
                LOGGER.Error("消息ID:{} 找不到对应的Msg.", msgId);
            }
            else
            {
                var message = Serializer.Deserialize<Message>(payload.Slice(16));
                if (message.MsgId != msgId)
                {
                    throw new Exception($"解析消息错误，注册消息id和消息无法对应.real:{message.MsgId}, register:{msgId}");
                }
                msg = message;
            }
            input = input.Slice(input.GetPosition(msgLen));
            return true;
        }

        public bool CheckMagicNumber(int order, int msgLen)
        {
            order ^= 0x1234 << 8;
            order ^= msgLen;

            if (lastOrder != 0 && order != lastOrder + 1)
            {
                LOGGER.Error("包序列出错, order=" + order + ", lastOrder=" + lastOrder);
                return false;
            }
            lastOrder = order;
            return true;
        }

        /// <summary>
        /// 检查消息长度是否合法
        /// </summary>
        /// <param name="msgLen"></param>
        /// <returns></returns>
        public bool CheckMsgLen(int msgLen)
        {
            //消息长度+时间戳+magic+消息id+数据
            //4 + 8 + 4 + 4 + data
            if (msgLen <= 16)//(消息长度已经被读取)
            {
                LOGGER.Error("从客户端接收的包大小异常:" + msgLen + ":至少16个字节");
                return false;
            }
            else if (msgLen > MAX_RECV_SIZE)
            {
                LOGGER.Error("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值：" + MAX_RECV_SIZE / 1024 + "字节");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 时间戳检查(可以防止客户端游戏过程中修改时间)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool CheckTime(long time)
        {
            if (lastReviceTime > time)
            {
                LOGGER.Error("时间戳出错，time=" + time + ", lastTime=" + lastReviceTime);
                return false;
            }
            lastReviceTime = time;
            return true;
        }

        public override void Write(Message msg)
        {
            if (IsClose())
                return;
            var bytes = Serializer.Serialize(msg);
            int len = 8 + bytes.Length;
            Span<byte> span = stackalloc byte[len];
            int offset = 0;
            span.WriteInt(len, ref offset);
            span.WriteInt(msg.MsgId, ref offset);
            span.WriteBytesWithoutLength(bytes, ref offset);

            lock (Writer)
            {
                Writer.Write(span);
            }

            if (sendSemaphore.CurrentCount == 0)
                sendSemaphore.Release();
        }

        public override void Close()
        {
            if (IsClose())
                return;

            lock (this)
            {
                if (Context == null)
                    return;
                try
                {
                    sendSemaphore.Release(short.MaxValue);
                    sendSemaphore?.Dispose();
                }
                catch
                {

                }

                try
                {
                    Context.Abort();
                }
                catch { }
                try
                {
                    Context.DisposeAsync();
                }
                catch { }
                Context = null;
            }
        }

        public override bool IsClose()
        {
            return Context == null || Context.ConnectionClosed.IsCancellationRequested;
        }
    }
}
