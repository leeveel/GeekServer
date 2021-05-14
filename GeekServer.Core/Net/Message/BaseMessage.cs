using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Geek.Server
{
    //可能用于数据库结果
    [BsonIgnoreExtraElements(true, Inherited = true)]//忽略代码删除的字段[数据库多余的字段]
    public abstract class BaseMessage : BaseState, IMessage
    {
        public virtual int Read(byte[] buffer, int offset)
        {
            return offset;
        }

        public virtual int Write(byte[] buffer, int offset)
        {
            return offset;
        }

        public virtual int GetMsgId()
        {
            return 0;
        }

        public byte[] Serialize()
        {
            return writeAsBuffer(512);
        }

        byte[] writeAsBuffer(int size)
        {
            var data = new byte[size];
            var offset = 0;
            offset = this.Write(data, offset);
            if(offset <= data.Length)
            {
                if(offset < data.Length)
                {
                    var ret = new byte[offset];
                    Array.Copy(data, 0, ret, 0, offset);
                    data = ret;
                }
                return data;
            }
            else
            {
                return writeAsBuffer(offset);
            }
        }

        public void Deserialize(byte[] data)
        {
            this.Read(data, 0);
        }
    }
}