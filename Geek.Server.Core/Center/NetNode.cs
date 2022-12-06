using MessagePack;

namespace Geek.Server.Core.Center
{

    public enum NodeType
    {
        Client = 1,
        Login = 2,
        Game = 3,
        Center = 4,
        Gateway = 5,
        GatewaySelect = 6
    }

    [MessagePackObject(true)]
    public class NetNode
    {
        public int NodeId { get; set; }
        public int ServerId { get; set; }
        public NodeType Type { get; set; }
        public string Ip { get; set; }
        public int TcpPort { get; set; }
        public int InnerTcpPort { get; set; }
        public int HttpPort { get; set; }
        public int RpcPort { get; set; }
        public List<string> ActorTypes { get; set; }
        [IgnoreMember]
        public string ActorTypesStr
        {
            get
            {
                return ActorTypes == null ? "" : string.Join(",", ActorTypes);
            }
        }
        public NetNodeState State { get; set; } = new NetNodeState();
        [IgnoreMember]
        public ICenterRpcHub rpcHub;
    }

    [MessagePackObject(true)]
    public class NetNodeState
    {
        //承载上限
        public int MaxLoad = int.MaxValue;
        //当前承载
        public int CurrentLoad = 0;
        //是否可以提供服务
        public bool CanServe = true;
    }
}
