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
            while (!cancelSrc.IsCancellationRequested)
            {
                try
                {
                    var result = await Reader.ReadAsync(cancelSrc.Token);
                    var buffer = result.Buffer;
                    if (buffer.Length > 0)
                    {
                        //LOGGER.Info($"读取tcp,len:{buffer.Length}");
                        SequencePosition examined = buffer.Start;
                        SequencePosition consumed = examined;
                        TryParseMessage(buffer, ref consumed, ref examined);
                        Reader.AdvanceTo(consumed, examined);
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

        void TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined)
        {
            var reader = new SequenceReader<byte>(input);

            if (!reader.TryReadBigEndian(out int msgLen))
            {
                examined = input.End; //告诉read task，到这里为止还不满足一个消息的长度，继续等待更多数据
                return;
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
                examined = input.End;
                return;
            }

            reader.TryRead(out byte flag);
            reader.TryReadBigEndian(out long netId);
            reader.TryReadBigEndian(out int serverId);
            var dataLen = msgLen - 13;
            if (dataLen > 0)
            {
                var payload = input.Slice(reader.Position, dataLen);
                Span<byte> data = stackalloc byte[dataLen];
                payload.CopyTo(data);
                consumed = examined = payload.End;
                onRecvPack(this, new TempNetPackage(flag, netId, serverId, data));
            }
            else
            {
                consumed = examined = reader.Position;
                onRecvPack(this, new TempNetPackage(flag, netId, serverId));
            }
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
            //LOGGER.Info($"tcp channel write:{package.ToString()}");
            Span<byte> target = stackalloc byte[package.Length + 4];
            int offset = 0;
            target.Write(package.Length, ref offset);
            target.Write(package, ref offset);
            Writer.Write(target);
            Writer.FlushAsync(cancelSrc.Token);
        }
    }
}
