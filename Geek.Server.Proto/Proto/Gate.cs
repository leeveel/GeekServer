using MessagePack;

namespace Geek.Server.Proto
{
    /// <summary>
    /// 请求转发消息
    /// </summary>
    [MessagePackObject(true)]
    public class ReqRouterMsg : Message
    {
        public long TargetUid { get; set; }
    }

    /// <summary>
    /// 返回请求转发结果
    /// </summary>
    [MessagePackObject(true)]
    public class ResRouterMsg : Message
    {
        public bool Result { get; set; }
    }

    /// <summary>
    /// 通知客户端服务节点没有连接
    /// </summary>
    [MessagePackObject(true)]
    public class ServerNotConnect : Message
    {
        public long serverUid { get; set; }
    }
}