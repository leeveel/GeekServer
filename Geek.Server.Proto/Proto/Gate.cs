using MessagePack;

namespace Geek.Server.Proto
{

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


    [MessagePackObject(true)]
    public class PlayerDisconnected : Message
    {
        /// <summary>
        /// 网关网络节点
        /// </summary>
        public int GateNodeId { get; set; }
    }

}