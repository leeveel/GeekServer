using System;
using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class BaseTcpHandler
    {
        /// <summary>
        /// 连接上下文
        /// </summary>
        public NetChannel Channel { get; set; }

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
                    return Msg.MsgId;
            }
        }

        /// <summary>
        /// 消息体
        /// </summary>
        public IMessage Msg { get; set; }

        public abstract Task ActionAsync();

        protected virtual void WriteAndFlush(NMessage msg)
        {
            if (IsDisconnectChannel(Channel))
                return;
            _ = Channel.WriteAsync(msg);
        }

        protected virtual void WriteAndFlush(int msgId, byte[] data)
        {
            if (msgId > 0 && data != null)
                WriteAndFlush(new NMessage(msgId, data));
        }

        bool IsDisconnectChannel(NetChannel ctx)
        {
            return ctx == null || ctx.Context == null;
        }
    }
}
