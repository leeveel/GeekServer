namespace Bedrock.Framework.Protocols
{
    public readonly struct ProtocolReadResult<TMessage>
    {
        public ProtocolReadResult(TMessage message, bool isCompleted)
        {
            Message = message;
            IsCompleted = isCompleted;
        }

        public TMessage Message { get; }
        public bool IsCompleted { get; }
    }
}
