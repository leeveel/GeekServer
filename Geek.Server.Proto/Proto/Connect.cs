using MessagePack; 

namespace ClientProto
{ 

    [MessagePackObject(true)]
    public class NetConnectMessage : Message
    { 
    }

    [MessagePackObject(true)]
    public class NetDisConnectMessage : Message
    { 
    } 
}
