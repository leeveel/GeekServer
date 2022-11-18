
using Geek.Server.App.Common;

namespace Geek.Server
{
    public class Session
    {
        /// <summary>
        /// 全局标识符
        /// </summary>
        public long Id { set; get; }

        /// <summary>
        /// 在网关中的id
        /// </summary>
        public long ClientConnId { get; set; }

        /// <summary>
        /// 网关节点id
        /// </summary>
        public int GateNodeId { get; set; }

        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime Time { set; get; }

        /// <summary>
        /// 连接标示，避免自己顶自己的号,客户端每次启动游戏生成一次/或者每个设备一个
        /// </summary>
        public string Token { get; set; }

        public void WriteAsync(Message msg)
        {
            var channel = GetNetChannel();
            var nmsg = new NMessage(msg)
            {
                ClientConnId = ClientConnId
            };
            channel?.WriteAsync(nmsg);
        }

        public NetChannel GetNetChannel()
        {
            var session = AppNetMgr.GetClientByNodeId(GateNodeId);
            return session?.Channel;
        }

    }
}