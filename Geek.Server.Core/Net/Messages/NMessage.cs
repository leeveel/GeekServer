using System.Buffers;

namespace Geek.Server
{
    /// <summary>
    /// net message
    /// </summary>
    public struct NMessage
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public ReadOnlySequence<byte> Payload { get; } = default;

        public NMessage(ReadOnlySequence<byte> payload)
        {
            Payload = payload;
        }

        public int MsgId = 0;

        public byte[] MsgRaw = null;

        public long TargetId = 0;
        public Message Msg { get; } = null;

        public NMessage(int msgId, byte[] msgData)
        {
            this.MsgId = msgId;
            MsgRaw = msgData;
        }

        public NMessage(Message msg)
        {
            Msg = msg;
            MsgId = msg.MsgId;
        }

        public void Serialize(IBufferWriter<byte> writer)
        {
            MessagePack.MessagePackSerializer.Serialize(writer, Msg);
        }

        public byte[] Serialize()
        {
            try
            {
                return MessagePack.MessagePackSerializer.Serialize(Msg);
            }
            catch (System.Exception e)
            {
                LOGGER.Fatal($"序列化失败:{Msg.GetType().FullName},{e}");
                return null;
            }
        }

        public byte[] GetBytes()
        {
            var bytes = MsgRaw;
            if (bytes == null)
            {
                bytes = Serialize();
            }
            return bytes;
        }

    }
}
