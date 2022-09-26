
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
        public Message Msg { get; set; }

        public abstract Task ActionAsync();

        public virtual void WriteAsync(NMessage msg)
        {
            if (IsDisconnectChannel(Channel))
                return;
            _ = Channel.WriteAsync(msg);
        }

        public virtual void WriteAsync(Message msg)
        {
            if (msg != null)
                WriteAsync(new NMessage(msg));
        }

        bool IsDisconnectChannel(NetChannel ctx)
        {
            return ctx == null || ctx.Context == null;
        }
    }
}
