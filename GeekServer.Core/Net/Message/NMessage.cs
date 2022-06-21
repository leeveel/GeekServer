using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Geek.Server
{
    /// <summary>
    /// net message
    /// </summary>
    public struct NMessage
    {

        public bool Ziped { get; set; } = false;

        public ReadOnlySequence<byte> Payload { get; } = default;

        public NMessage(ReadOnlySequence<byte> payload, bool isZip)
        {
            Payload = payload;
            Ziped = isZip;
        }

        /// <summary>
        /// 对超过1K的数据进行压缩
        /// 压缩会造成更多的拷贝和GC需要自己权衡
        /// </summary>
        public const int ZipThreshold = 1024;

        public BaseMessage Msg { get; } = null;

        public byte[] Compressed { get; } = null;

        private int serializeLength = 0;
        public NMessage(BaseMessage msg)
        {
            Msg = msg;
            serializeLength = msg.GetSerializeLength();
            if (serializeLength > ZipThreshold)
            {
                Ziped = true;
                var before = ArrayPool<byte>.Shared.Rent(serializeLength);
                msg.Serialize(before);
                Compressed = MsgDecoder.CompressGZip(before, serializeLength);
                ArrayPool<byte>.Shared.Return(before);
            }
        }

        public int GetSerializeLength()
        {
            if (Ziped)
                return Compressed.Length;
            else
                return serializeLength;
        }

        public void Serialize(Span<byte> span, int offset=0)
        {
            Msg.Serialize(span, offset);
        }

    }
}
