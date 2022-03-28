using System;
using System.Collections.Generic;

namespace Geek.Client
{

    /// <summary>
    /// 除了消息头和State头以外的全部继承此类
    /// </summary>
    public abstract class Serializable : ISerializable
    {

        public virtual int Read(byte[] buffer, int offset)
        {
            return offset;
        }

        public virtual int Write(byte[] buffer, int offset)
        {
            return offset;
        }

        public byte[] Serialize()
        {
            return WriteAsBuffer(512);
        }

        protected virtual byte[] WriteAsBuffer(int size)
        {
            var data = new byte[size];
            var offset = 0;
            offset = this.Write(data, offset);
            if (offset <= data.Length)
            {
                if (offset < data.Length)
                {
                    var ret = new byte[offset];
                    Array.Copy(data, 0, ret, 0, offset);
                    data = ret;
                }
                return data;
            }
            else
            {
                return WriteAsBuffer(offset);
            }
        }

        public void Deserialize(byte[] data)
        {
            this.Read(data, 0);
        }

        public virtual int Sid { get; }

        public virtual T Create<T>(int sid) where T : Serializable
        {
            return null;
        }

        protected virtual List<T> GetList<T>()
        {
            return new List<T>();
        }

        protected virtual HashSet<T> GetSet<T>()
        {
            return new HashSet<T>();
        }

        protected virtual Dictionary<K, V> GetMap<K, V>()
        {
            return new Dictionary<K, V>();
        }


        protected virtual T ReadCustom<T>(T target, bool optional, byte[] buffer, ref int offset) where T : Serializable
        {
            if (optional)
            {
                var hasVal = XBuffer.ReadBool(buffer, ref offset);
                if (hasVal)
                {
                    var sid = XBuffer.ReadInt(buffer, ref offset);
                    target = Create<T>(sid);
                    offset = target.Read(buffer, offset);
                }
            }
            else
            {
                var sid = XBuffer.ReadInt(buffer, ref offset);
                target = Create<T>(sid);
                offset = target.Read(buffer, offset);
            }
            return target;
        }

        protected virtual int WriteCustom<T>(T target, bool optional, byte[] buffer, ref int offset) where T : Serializable
        {
            if (optional)
            {
                bool hasVal = target != default;
                XBuffer.WriteBool(hasVal, buffer, ref offset);
                if (hasVal)
                {
                    XBuffer.WriteInt(target.Sid, buffer, ref offset);
                    offset = target.Write(buffer, offset);
                }
            }
            else
            {
                XBuffer.WriteInt(target.Sid, buffer, ref offset);
                offset = target.Write(buffer, offset);
            }
            return offset;
        }

    }
}
