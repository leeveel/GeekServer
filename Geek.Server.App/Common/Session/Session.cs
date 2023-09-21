
using Geek.Server.Core.Net;

namespace Geek.Server.App.Common.Session
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

        public void WriteAsync(Message msg)
        {
            Channel?.Write(msg);
        }
    }
}