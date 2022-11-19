using MessagePack;


namespace Geek.Server
{
    /// <summary>
    /// 此类主要用于与Core工程解耦
    /// 需要和Core工程的Message命名空间保持一致
    /// </summary>
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
