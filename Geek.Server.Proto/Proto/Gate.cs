using MessagePack;

namespace Geek.Server.Proto
{
    /// <summary>
    /// 请求转发消息
    /// </summary>
    //[MessagePackObject(true)]
    //public class ReqRouterMsg : Message
    //{
    //    public long TargetUid { get; set; }
    //}

    ///// <summary>
    ///// 返回请求转发结果
    ///// </summary>
    //[MessagePackObject(true)]
    //public class ResRouterMsg : Message
    //{
    //    public bool Result { get; set; }
    //}

    /// <summary>
    /// 通知客户端服务节点没有连接
    /// </summary>
    [MessagePackObject(true)]
    public class ServerNotConnect : Message
    {
        public long serverUid { get; set; }
    }


    [MessagePackObject(true)]
    public class ReqConnectGate : Message
    {
        public int ServerId { get; set; }
    }

    [MessagePackObject(true)]
    public class ResConnectGate : Message
    {
        /// <summary>
        /// 服务器id
        /// </summary>
        public int ServerId { get; set; }
        /// <summary>
        /// 节点id(单服结构时==ServerId)
        /// </summary>
        public long NodeId { get; set; }
    }

    [MessagePackObject(true)]
    public class NodeNotFound : Message
    {
        public int NodeId { get; set; }
    }



    [MessagePackObject(true)]
    public class ReqInnerConnectGate : Message
    {
        public int NodeId { get; set; }
    }

    [MessagePackObject(true)]
    public class ResInnerConnectGate : Message
    {
        public bool IsSuccess { get; set; }
    }

}