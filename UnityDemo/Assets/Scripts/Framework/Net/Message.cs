using MessagePack;

namespace Geek.Server
{
    [MessagePackObject(false)]
    public abstract class Message
    {
        /// <summary>
        /// 消息唯一id
        /// </summary>
        public int UniId { get; set; }
        [IgnoreMember]
        public virtual int MsgId { get; }
    }
}
