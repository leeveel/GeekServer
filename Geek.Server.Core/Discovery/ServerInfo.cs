using MessagePack;
using Newtonsoft.Json;
using PolymorphicMessagePack;
using System.Net;

namespace Core.Discovery
{
    [MessagePackObject(true)]
    [PolymorphicIgnore]
    public class ServerInfo
    {
        public string ServerName { get; set; } 
        public int ServerId { get; set; }
        public ServerType Type { get; set; }
        public string PublicIp { get; set; }
        public string LocalIp { get; set; }
        public int InnerPort { get; set; }
        public int OuterPort { get; set; }
        public int HttpPort { get; set; }
        public int RpcPort { get; set; }

        [JsonIgnore]
        public ServerState State { get; set; } = new ServerState();

        [JsonIgnore]
        [MessagePack.IgnoreMember]
        public EndPoint InnerEndPoint { get; set; }

        public bool IsSame(ServerInfo info)
        {
            return ToString() == info.ToString();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [MessagePackObject(true)]
    [PolymorphicIgnore]
    public class ServerState
    {
        //承载上限
        public int MaxLoad = int.MaxValue;
        //当前承载
        public int CurrentLoad = 0;

        public override string ToString()
        {
            return $"MaxLoad:{MaxLoad},CurrentLoad:{CurrentLoad}";
        }
    }
}
