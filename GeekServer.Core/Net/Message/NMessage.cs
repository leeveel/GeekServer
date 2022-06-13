using System.Buffers;
using System.Runtime.InteropServices;

namespace Geek.Server
{
    /// <summary>
    /// net message
    /// </summary>
    public struct NMessage
    {
        public int MsgId { get; set; } = 0;

        public bool Ziped { get; set; } = false;

        public ReadOnlySequence<byte> Payload { get; }

        public NMessage(ReadOnlySequence<byte> payload, bool isZip)
        {
            Payload = payload;
            Ziped = isZip;
        }

        /// <summary>
        /// 对超过1K的数据进行压缩
        /// </summary>
        public const int ZipThreshold = 1024;

        public NMessage(int msgId, byte[] payload)
        {
            int len = payload.Length;
            if (len >= ZipThreshold)
            {
                Ziped = true;
                payload = MsgDecoder.CompressGZip(payload);
                //Console.WriteLine($"msg:{msgId} zip before:{len}, after:{payload.Length}");
            }
            Payload = new ReadOnlySequence<byte>(payload);
            MsgId = msgId;
        }

        public static NMessage Create(int msgId, byte[] payload)
        {
            return new NMessage(msgId, payload);
        }

        public void Dispose()
        {
            foreach (var item in Payload)
            {
                if (MemoryMarshal.TryGetArray(item, out var segment))
                {
                    ArrayPool<byte>.Shared.Return(segment.Array);
                }
            }
        }

    }
}
