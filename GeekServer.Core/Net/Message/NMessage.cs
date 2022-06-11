using System.Buffers;

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

        public NMessage(int msgId, PooledBuffer payload)
        {
            int len = payload.RealLength;
            if (len >= ZipThreshold)
            {
                Ziped = true;
                var after = MsgDecoder.CompressGZip(payload);
                //Console.WriteLine($"msg:{msgId} zip before:{len}, after:{payload.Length}");
                Payload = new ReadOnlySequence<byte>(after);
            }
            else
            {
                Payload = new ReadOnlySequence<byte>(payload.NonRedundantBuffer());
            }
            MsgId = msgId;
        }


        public static NMessage Create(int msgId, byte[] payload)
        {
            return new NMessage(msgId, payload);
        }

    }
}
