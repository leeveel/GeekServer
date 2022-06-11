using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Geek.Server
{
    public struct PooledBuffer
    {
        /// <summary>
        /// 池化的buffer
        /// </summary>
        public byte[] Buffer { get; set; }
        /// <summary>
        /// 正式长度
        /// </summary>
        public int RealLength { get; set; }

        public PooledBuffer(byte[] buffer, int realLen)
        {
            Buffer = buffer;
            RealLength = realLen;
        }


        /// <summary>
        /// 非冗余数据
        /// </summary>
        /// <returns></returns>
        public byte[] NonRedundantBuffer()
        {
            if (RealLength == Buffer.Length)
            {
                return Buffer;
            }
            else
            {
                var ret = new byte[RealLength];
                Array.Copy(Buffer, 0, ret, 0, RealLength);
                ArrayPool<byte>.Shared.Return(Buffer); //归还
                Buffer = ret;
                return ret;
            }
        }
    }


    public interface ISerializable
    {
        int Read(byte[] buffer, int offset);

        int Write(byte[] buffer, int offset);

        byte[] Serialize();

        PooledBuffer SerializeToPool();

        void Deserialize(byte[] data);
    }
}
