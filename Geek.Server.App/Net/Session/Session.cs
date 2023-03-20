using Geek.Server.Core.Net.Tcp;

namespace Geek.Server.App.Net.Session
{
    public class Session
    {
        /// <summary>
        /// 角色id
        /// </summary>
        public long RoleId { set; get; }

        /// <summary>
        /// 在网关中的网络id
        /// </summary>
        public long NetId { get; set; }

        /// <summary>
        /// 与网关连接的channel
        /// </summary>
        public NetChannel Channel { get; set; }

        /// <summary>
        /// 连接标示，避免自己顶自己的号,客户端每次启动游戏生成一次/或者每个设备一个
        /// </summary>
        public string Token { get; set; }

        public void Write(Message msg)
        {
            var nmsg = new NetMessage
            {
                NetId = NetId,
                Msg = msg,
                MsgId = msg.MsgId
            };
            Channel?.Write(nmsg);
        }
    }
}