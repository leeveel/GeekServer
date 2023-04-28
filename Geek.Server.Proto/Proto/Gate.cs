using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
    //客户端连接成功后，需要主动推送此消息
    [MessagePackObject(true)]
    public class ReqConnectGate : Message
    {
        public int ServerId { get; set; }
    }

    [MessagePackObject(true)]
    public class ResConnectGate : Message
    {
        public bool Result { get; set; }
    }

    //逻辑服连接后，主动推送给网关服
    [MessagePackObject(true)]
    public class ReqInnerConnectGate : Message
    {
        public int NetId { get; set; }
    }

    [MessagePackObject(true)]
    public class ResInnerConnectGate : Message
    {
        public bool IsSuccess { get; set; }
        //当前逻辑服的客户端id ，当逻辑服断线重连的时候，需要同步此数据给逻辑服
        public List<long> ClientIds { get; set; }
    }

    //客户端连接网关后，通知逻辑服
    [MessagePackObject(true)]
    public class ReqClientChannelActive : Message
    {
        public string Address { get; set; }
    }

    //客户端断开后，通知逻辑服
    [MessagePackObject(true)]
    public class ReqClientChannelInactive : Message
    {
    }

    //逻辑服主动要求断开客户端连接，通知网关服
    [MessagePackObject(true)]
    public class ReqDisconnectClient : Message
    {
        public long NetId { get; set; }
    }

}