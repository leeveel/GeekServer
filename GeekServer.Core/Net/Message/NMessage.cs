using System.Buffers;

namespace Geek.Server
{
    /// <summary>
    /// net message
    /// </summary>
    public struct NMessage
    {

        public ReadOnlySequence<byte> Payload { get; } = default;

        public NMessage(ReadOnlySequence<byte> payload)
        {
            Payload = payload;
        }

        public BaseMessage Msg { get; } = null;

        public NMessage(BaseMessage msg)
        {
            Msg = msg;
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
            catch (System.Exception)
            {
                throw;
            }
        }

    }
}
