using System.Buffers;

namespace Geek.Server.Core.Net.Bedrock.Protocols
{
    public interface IMessageWriter<TMessage>
    {
        void WriteMessage(TMessage message, IBufferWriter<byte> output);
    }
}
