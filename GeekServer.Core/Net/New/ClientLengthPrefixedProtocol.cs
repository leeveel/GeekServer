using System;
using System.Buffers;
using Bedrock.Framework.Protocols;

namespace Geek.Server
{
    public class ClientLengthPrefixedProtocol : IProtocal<Message>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out Message message)
        {
            var reader = new SequenceReader<byte>(input);
            if (!reader.TryReadBigEndian(out int length) || reader.Remaining < length-4)
            {
                message = default;
                return false;
            }

            var payload = input.Slice(reader.Position, length-4);//length已经被TryReadBigEndian读取
            message = new Message(payload);

            consumed = payload.End;
            examined = consumed;
            return true;
        }

        private const int Magic = 0x1234;
        int count = 0;
        public void WriteMessage(Message message, IBufferWriter<byte> output)
        {
            //length + timestamp + magic + msgid
            int len = 4 + 8 + 4 + 4 + (int)message.Payload.Length;
            var header = output.GetSpan(20);

            int magic = Magic + ++count;
            magic ^= Magic << 8;
            magic ^= len;

            int offset = 0;
            XBuffer.WriteInt(header, len, ref offset);
            XBuffer.WriteLong(header, DateTime.Now.Ticks / 10000, ref offset);
            XBuffer.WriteInt(header, magic, ref offset);
            XBuffer.WriteInt(header, message.MsgId, ref offset);
            output.Advance(20);
            foreach (var memory in message.Payload)
            {
                output.Write(memory.Span);
            }
        }
    }

}
