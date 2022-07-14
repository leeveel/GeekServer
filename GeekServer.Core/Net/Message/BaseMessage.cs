using MessagePack;

namespace Geek.Server
{
    [MessagePackObject]
    public abstract class BaseMessage
    {
        /// <summary>
        /// 消息唯一id
        /// </summary>
        [Key(0)]
        public int UniId { get; set; }
        [IgnoreMember]
        public virtual int MsgId { get; }
    }
}
