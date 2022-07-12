using Geek.Server;
using System.Buffers;

namespace Geek.Client
{
    /// <summary>
    /// net message
    /// </summary>
    public class NMessage
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
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
                throw;
            }
        }

    }
}
