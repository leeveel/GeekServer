using Geek.Server.Core.Net;
using Geek.Server.Core.Net.Kcp;
using MessagePack;
using Microsoft.AspNetCore.Connections;
using System.Buffers;
using System.IO.Pipelines;

namespace Geek.Server.Gateway.Outer
{
    public class TcpChannel : BaseNetChannel
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public delegate void ReceiveFunc(TcpChannel channel, TempNetPackage package);
        public ConnectionContext Context { get; protected set; }
        protected PipeReader Reader { get; set; }
        protected PipeWriter Writer { get; set; }

        private readonly SemaphoreSlim sendSemaphore = new(0);
        private readonly MemoryStream sendStream = new();

        protected string remoteAddress;
        protected ReceiveFunc onRecvPack;
        protected CancellationTokenSource cancelSrc;

        public TcpChannel(ConnectionContext context, ReceiveFunc onRecvPack)
        {
            Context = context;
            Reader = context.Transport.Input;
            Writer = context.Transport.Output;
            this.onRecvPack = onRecvPack;
            remoteAddress = context.RemoteEndPoint?.ToString();
            UpdateRecvMessageTime(TimeSpan.FromSeconds(5).Ticks);
            cancelSrc = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            _ = TrySendAsync();
            while (!cancelSrc.IsCancellationRequested)
            {
                try
                {
                    var result = await Reader.ReadAsync(cancelSrc.Token);
                    var buffer = result.Buffer;
                    if (buffer.Length > 0)
                    {
                        int count = 0;
                        while (TryParseMessage(ref buffer))
                        {
                            if (++count > 100)
                            {
                                await Task.Delay(5);
                            }
                        }
                        Reader.AdvanceTo(buffer.Start, buffer.End);
                    }
                    else if (result.IsCanceled || result.IsCompleted)
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

        async Task TrySendAsync()
        {
            //pipewriter线程不安全，这里统一发送写刷新数据
            try
            {
                var token = cancelSrc.Token;
                while (!token.IsCancellationRequested)
                {
                    await sendSemaphore.WaitAsync();
                    lock (sendStream)
                    {
                        var len = sendStream.Length;
                        if (len > 0)
                        {
                            Writer.Write(sendStream.GetBuffer().AsSpan<byte>()[..(int)len]);
                            sendStream.SetLength(0);
                            sendStream.Position = 0;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    await Writer.FlushAsync(token);
                }
            }
            catch { };
        }

        bool TryParseMessage(ref ReadOnlySequence<byte> input)
        {
            var reader = new SequenceReader<byte>(input);

            if (!reader.TryReadBigEndian(out int msgLen))
            {
                return false;
            }

            if (msgLen < 13)//(消息长度已经被读取)
            {
                throw new Exception($"从客户端接收的包大小异常,{msgLen} {reader.Remaining}:至少大于8个字节");
            }
            else if (msgLen > 1500)
            {
                throw new Exception("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值：" + 1500 + "字节");
            }

            if (reader.Remaining < msgLen)
            {
                return false;
            }

            reader.TryRead(out byte flag);
            reader.TryReadBigEndian(out long netId);
            reader.TryReadBigEndian(out int serverId);
            var dataLen = msgLen - 13; 
            var payload = input.Slice(reader.Position, dataLen);
            Span<byte> data = stackalloc byte[dataLen];
            payload.CopyTo(data);
            onRecvPack(this, new TempNetPackage(flag, netId, serverId, data));
            input = input.Slice(msgLen + 4);
            return true;
        }

        public override bool IsClose()
        {
            return Context == null;
        }

        public override void Close()
        {
            lock (this)
            {
                try
                {
                    if (Context == null)
                        return;
                    cancelSrc.Cancel();
                    Context?.Abort();
                    sendStream.Close();
                }
                catch
                {

                }
                finally
                {
                    Reader = null;
                    Writer = null;
                    Context = null;
                }
            }
        }


        public override void Write(TempNetPackage package)
        {
            lock (sendStream)
            {
                sendStream.Write(package);
            }
            sendSemaphore.Release();
        }
    }
}
