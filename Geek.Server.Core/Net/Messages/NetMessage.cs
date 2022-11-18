using System.Buffers;

namespace Geek.Server.Core.Net.Messages
{
    /// <summary>
    /// net message
    /// </summary>
    public struct NetMessage
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public ReadOnlySequence<byte> Payload { get; } = default;

        public NetMessage(ReadOnlySequence<byte> payload)
        {
            Payload = payload;
        }

        public int MsgId = 0;

        public byte[] MsgRaw = null;

        /// <summary>
        /// 客户端网络连接ID
        /// </summary>
        public long ClientConnId = 0;

        public Message Msg { get; } = null;

        public NetMessage(int msgId, byte[] msgData)
        {
            MsgId = msgId;
            MsgRaw = msgData;
        }

        public NetMessage(Message msg)
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
            catch (Exception e)
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
