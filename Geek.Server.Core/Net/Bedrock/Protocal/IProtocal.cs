using System;
using System.Buffers;

namespace Bedrock.Framework.Protocols
{
    public interface IProtocal<TMessage>
    {
        bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out TMessage message);
        void WriteMessage(TMessage message, IBufferWriter<byte> output);
    }
}
