using System;
using System.Buffers;
using Bedrock.Framework.Protocols;
using GeekServer.Gateaway.Util;
using Microsoft.AspNetCore.Connections;

namespace GeekServer.Gateaway.Net.Tcp
{
    public class MessageProtocol : IProtocal<NetMessage>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out NetMessage message)
        {
            var reader = new SequenceReader<byte>(input);

            if (!reader.TryReadBigEndian(out int length) || reader.Remaining < length - 4)
            {
                message = default;
                return false;
            }

            var payload = input.Slice(reader.Position, length - 4);
            message = new NetMessage(payload);
            consumed = payload.End;
            examined = consumed;
            return true;
        }


        public void WriteMessage(NetMessage message, IBufferWriter<byte> output)
        {
            var bytes = message.MsgRaw;
            int len = 8 + bytes.Length;
            var span = output.GetSpan(len);
            int offset = 0;
            XBuffer.WriteInt(len, span, ref offset);
            XBuffer.WriteInt(message.MsgId, span, ref offset);
            XBuffer.WriteBytesWithoutLength(bytes, span, ref offset);
            output.Advance(len);
        }
    }
}
