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
    }

}
