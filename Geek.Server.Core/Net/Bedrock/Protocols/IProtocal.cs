namespace Geek.Server.Core.Net.Bedrock.Protocols
{
    public interface IProtocal<TMessage> : IMessageReader<TMessage>, IMessageWriter<TMessage>
    {
    }
}
