using System;
using System.Buffers;
using System.IO.Compression;

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

        public NMessage(ReadOnlySequence<byte> payload)
        {
            Payload = payload;
        }

        public NMessage(int msgId, byte[] payload)
        {
            int len = payload.Length;
            if (len >= 512)
            {
                Ziped = true;
                payload = MsgDecoder.CompressGZip(payload);
                //LOGGER.Debug($"msg:{msg.MsgId} zip before:{len}, after:{msgData.Length}");
            }
            Payload = new ReadOnlySequence<byte>(payload);
            MsgId = msgId;
        }

        public static NMessage Create(int msgId, byte[] payload)
        {
            return new NMessage(msgId, payload);
        }

    }
}
