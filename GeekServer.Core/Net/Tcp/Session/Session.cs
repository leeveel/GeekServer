﻿
namespace Geek.Server
{
    public class Session
    {
        /// <summary>
        /// 全局标识符
        /// </summary>
        public long Id { set; get; }

        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime Time { set; get; }

        /// <summary>
        /// 连接上下文
        /// </summary>
        public NetChannel Channel { get; set; }

        /// <summary>
        /// 连接标示，避免自己顶自己的号,客户端每次启动游戏生成一次/或者每个设备一个
        /// </summary>
        public string Sign { get; set; }

        public void Write(NetChannel ctx, NMessage msg)
        {
            if (IsDisconnect(ctx))
                return;
            ctx.WriteAsync(msg);
        }

        public void Write(NetChannel ctx, Message msg)
        {
            if (IsDisconnect(ctx))
                return;
            NMessage smsg = new NMessage(msg);
            ctx.WriteAsync(smsg);
        }

        public bool IsDisconnect(NetChannel channel)
        {
            return channel == null || channel.Context == null;
        }

        public void CloseChannel(NetChannel channel)
        {
            if (channel != null)
                channel.Abort();
        }
    }
}