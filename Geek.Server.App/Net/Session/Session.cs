using Geek.Server.Core.Net;

namespace Geek.Server.App.Net.Session
{
    public class GameSession
    {
        /// <summary>
        /// 角色id
        /// </summary>
        public long RoleId { set; get; }

        /// <summary>
        /// 与网关连接的channel
        /// </summary>
        public BaseNetChannel Channel { get; set; }

        /// <summary>
        /// 连接标示，避免自己顶自己的号,客户端每次启动游戏生成一次/或者每个设备一个
        /// </summary>
        public string Sign { get; set; }

        public void Write(Message msg)
        {
            Channel?.Write(msg);
        }
    }
}