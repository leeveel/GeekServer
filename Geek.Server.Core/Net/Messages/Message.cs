using MessagePack;

namespace Geek.Server
{
    [MessagePackObject(true)]
    public abstract class Message
    {
        public int UniId { get; set; }
        [IgnoreMember]
        public virtual int MsgId { get; }
        [IgnoreMember]
        public long NetId { get; set; }
    }
}
