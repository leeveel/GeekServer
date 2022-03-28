using System;
using MongoDB.Bson.Serialization.Attributes;


namespace Geek.Server
{
    //可能用于数据库结果
    [BsonIgnoreExtraElements(true, Inherited = true)]//忽略代码删除的字段[数据库多余的字段]
    public abstract class BaseMessage : Serializable, IMessage
    {
        /// <summary>
        /// 消息唯一id
        /// </summary>
        [BsonIgnore]
        public int UniId { get; set; }
        [BsonIgnore]
        public int MsgId { get { return Sid; } }

        public virtual int WriteWithType(byte[] _buffer_, int _offset_)
        {
            return _offset_;
        }

        public virtual int GetMsgId()
        {
            return 0;
        }

    }
}