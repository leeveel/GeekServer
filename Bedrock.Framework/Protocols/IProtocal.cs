using System.Buffers;

namespace Bedrock.Framework.Protocols
{
    public interface IProtocal<TMessage> : IMessageReader<TMessage>, IMessageWriter<TMessage>
    {
    }
}
