using Geek.Server;
using NLog;
using NLog.Fluent;

namespace Geek.Server
{
    public class Session
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 全局标识符
        /// </summary>
        public long Id { set; get; }
        //在gate中的id
        public long netId { set; get; }
        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime Time { set; get; }

        public StreamClient streanClient { get; set; }

        /// <summary>
        /// 连接标示，避免自己顶自己的号,客户端每次启动游戏生成一次/或者每个设备一个
        /// </summary>
        public string Sign { get; set; }

        public void Abort()
        {
            streanClient.PlayerDisconnect(netId);
        }

        public void WriteAsync(Message msg)
        {
            Log.Info($"写入消息:{msg.MsgId} {msg.UniId}");
            streanClient.serverAgent?.Router(netId, msg.MsgId, MessagePack.MessagePackSerializer.Serialize(msg));
        }
    }
}