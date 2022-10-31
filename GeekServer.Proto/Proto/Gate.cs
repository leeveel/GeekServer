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
    /// 请求断开客户端连接
    /// </summary>
    [MessagePackObject(true)]
    public class ReqDisconnectClient : Message
    {
        public long TargetUid { get; set; }
    }
}