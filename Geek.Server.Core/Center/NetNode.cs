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
        Proxy = 6
    }

    [MessagePackObject]
    public class NetNode
    {
        [Key(0)]
        public int NodeId { get; set; }
        [Key(1)]
        public int ServerId { get; set; }
        [Key(2)]
        public NodeType Type { get; set; }
        [Key(3)]
        public string Ip { get; set; }
        [Key(4)]
        public int TcpPort { get; set; }
        [Key(5)]
        public int InnerTcpPort { get; set; }
        [Key(6)]
        public int HttpPort { get; set; }
        [Key(7)]
        public int RpcPort { get; set; }
        [Key(8)]
        public NetNodeState State { get; set; } = new NetNodeState();
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
