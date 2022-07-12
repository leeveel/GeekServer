//using System;
//using System.Buffers;

//namespace Geek.Server
//{

//    /// <summary>
//    /// 除了消息头和State头以外的全部继承此类
//    /// </summary>
//    public abstract class Serializable : BaseDBState, ISerializable
//    {

//        public virtual int Read(byte[] buffer, int offset)
//        {
//            return offset;
//        }

//        public virtual int Write(byte[] buffer, int offset)
//        {
//            return offset;
//        }

//        public virtual int Read(Span<byte> buffer, int offset)
//        {
//            return offset;
//        }

//        public virtual int Write(Span<byte> buffer, int offset)
//        {
//            return offset;
//        }

//        public virtual int GetSerializeLength()
//        {
//            throw new NotImplementedException();
//        }

//        public void Serialize(Span<byte> span, int offset = 0)
//        {
//            Write(span, offset);
//        }

//        public byte[] Serialize()
//        {
//            int size = GetSerializeLength();
//            var data = new byte[size];
//            int offset = 0;
//            offset = Write(data, offset);
//            if (offset != data.Length)
//                throw new Exception($"{GetType().FullName}GetSerializeLength 计算错误!");
//            return data;
//        }

//        public void Deserialize(byte[] data)
//        {
//            this.Read(data, 0);
//        }

//        public void Deserialize(Span<byte> data)
//        {
//            this.Read(data, 0);
//        }

//        public virtual int Sid { get; }

//        public virtual T Create<T>(int sid) where T : Serializable
//        {
//            return null;
//        }

//        protected virtual T ReadCustom<T>(T target, bool optional, byte[] buffer, ref int offset) where T : Serializable
//        {
//            if (optional)
//            {
//                var hasVal = XBuffer.ReadBool(buffer, ref offset);
//                if (hasVal)
//                {
//                    var sid = XBuffer.ReadInt(buffer, ref offset);
//                    target = Create<T>(sid);
//                    offset = target.Read(buffer, offset);
//                }
//            }
//            else
//            {
//                var sid = XBuffer.ReadInt(buffer, ref offset);
//                target = Create<T>(sid);
//                offset = target.Read(buffer, offset);
//            }
//            return target;
//        }

//        protected virtual T ReadCustom<T>(T target, bool optional, Span<byte> buffer, ref int offset) where T : Serializable
//        {
//            if (optional)
//            {
//                var hasVal = XBuffer.ReadBool(buffer, ref offset);
//                if (hasVal)
//                {
//                    var sid = XBuffer.ReadInt(buffer, ref offset);
//                    target = Create<T>(sid);
//                    offset = target.Read(buffer, offset);
//                }
//            }
//            else
//            {
//                var sid = XBuffer.ReadInt(buffer, ref offset);
//                target = Create<T>(sid);
//                offset = target.Read(buffer, offset);
//            }
//            return target;
//        }


//        protected virtual int WriteCustom<T>(T target, bool optional, byte[] buffer, ref int offset) where T : Serializable
//        {
//            if (optional)
//            {
//                bool hasVal = target != default;
//                XBuffer.WriteBool(hasVal, buffer, ref offset);
//                if (hasVal)
//                {
//                    XBuffer.WriteInt(target.Sid, buffer, ref offset);
//                    offset = target.Write(buffer, offset);
//                }
//            }
//            else
//            {
//                XBuffer.WriteInt(target.Sid, buffer, ref offset);
//                offset = target.Write(buffer, offset);
//            }
//            return offset;
//        }

//        protected virtual int WriteCustom<T>(T target, bool optional, Span<byte> buffer, ref int offset) where T : Serializable
//        {
//            if (optional)
//            {
//                bool hasVal = target != default;
//                XBuffer.WriteBool(hasVal, buffer, ref offset);
//                if (hasVal)
//                {
//                    XBuffer.WriteInt(target.Sid, buffer, ref offset);
//                    offset = target.Write(buffer, offset);
//                }
//            }
//            else
//            {
//                XBuffer.WriteInt(target.Sid, buffer, ref offset);
//                offset = target.Write(buffer, offset);
//            }
//            return offset;
//        }

//    }
//}
