using System;
using DotNetty.Transport.Channels;
using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class BaseTcpHandler
    {
        /// <summary>
        ///  连接
        /// </summary>
        public IChannelHandlerContext Ctx { get; set; }

        /// <summary>
        /// 从Decoder中转化出来的时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 消息id
        /// </summary>
        public int MsgId
        {
            get
            {
                if (Msg == null)
                    return 0;
                else
                    return Msg.GetMsgId();
            }
        }

        /// <summary>
        /// 消息体
        /// </summary>
        public IMessage Msg { get; set; }

        public abstract Task ActionAsync();

        protected virtual void WriteAndFlush(NMessage msg)
        {
            if (IsDisconnectChannel(Ctx))
                return;
            Ctx.WriteAndFlushAsync(msg);
        }

        protected virtual void WriteAndFlush(BaseMessage msg)
        {
            WriteAndFlush(msg.GetMsgId(), msg.Serialize());
        }

        protected virtual void WriteAndFlush(int msgId, byte[] data)
        {
            if (msgId > 0 && data != null)
                WriteAndFlush(new NMessage() { MsgId = msgId, Data = data });
        }



        bool IsDisconnectChannel(IChannelHandlerContext ctx)
        {
            return ctx == null || ctx.Channel == null || !ctx.Channel.Active || !ctx.Channel.Open;
        }
    }
}
