using MessagePack;
using System.Net;

namespace Geek.Server.Core.Center
{
    [MessagePackObject(true)]
    public class ServerInfo
    {
        public int ServerId { get; set; }
        public ServerType Type { get; set; }
        public string Ip { get; set; }
        public string InnerIp { get; set; }
        public int TcpPort { get; set; }
        public int InnerTcpPort { get; set; }
        public int HttpPort { get; set; }
        public int RpcPort { get; set; }
        public ServerState State { get; set; } = new ServerState();
        [IgnoreMember]
        public EndPoint InnerUdpEndPoint { get; set; }
    }

    [MessagePackObject(true)]
    public class ServerState
    {
        //承载上限
        public int MaxLoad = int.MaxValue;
        //当前承载
        public int CurrentLoad = 0;
    }
}
