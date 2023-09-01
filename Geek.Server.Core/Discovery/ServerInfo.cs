using MessagePack;
using System.Net;

namespace Core.Discovery
{
    [MessagePackObject(true)]
    public class ServerInfo
    {
        public int ServerId { get; set; }
        public ServerType Type { get; set; }
        public string Ip { get; set; }
        public string localIp { get; set; }
        public int InnerPort { get; set; }
        public int OuterPort { get; set; }
        public int HttpPort { get; set; }
        public int RpcPort { get; set; }
        public ServerState State { get; set; } = new ServerState();
        [IgnoreMember]
        public EndPoint InnerEndPoint { get; set; }
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
