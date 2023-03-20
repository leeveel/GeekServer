namespace Bedrock.Framework.Protocols
{
    public readonly struct ProtocolReadResult<TMessage>
    {
        public ProtocolReadResult(bool havaMsg, TMessage message, bool isCompleted)
        {
            Message = message;
            IsCompleted = isCompleted;
            HaveMsg = havaMsg;
        }

        public TMessage Message { get; }
        public bool IsCompleted { get; }
        public bool HaveMsg { get; }
    }
}
