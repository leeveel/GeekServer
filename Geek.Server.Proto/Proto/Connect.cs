using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
    public enum NetCode
    {
        Unknown = 0,
        Success,
        Fail,
        Closed,
    }

    [MessagePackObject(true)]
    public class NetConnectMessage : Message
    {
        public NetCode Code { get; set; }
    }

    [MessagePackObject(true)]
    public class NetDisConnectMessage : Message
    {
        public NetCode Code { get; set; }
    }
}
