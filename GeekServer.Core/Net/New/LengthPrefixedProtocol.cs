using System;
using System.Buffers;
using System.Buffers.Binary;
using Bedrock.Framework.Protocols;

namespace Geek.Server
{
    public class LengthPrefixedProtocol : IMessageReader<Message>, IMessageWriter<Message>
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

        public void WriteMessage(Message message, IBufferWriter<byte> output)
        {
            int len = (int)message.Payload.Length;
            len += 8;
            var header = output.GetSpan(8);                                         
            int offset = 0;                                      //负号，用于标记数据包是否压缩
            XBuffer.WriteInt(header, message.Ziped ? -len : len, ref offset);
            XBuffer.WriteInt(header, message.MsgId, ref offset);
            output.Advance(8);
            foreach (var memory in message.Payload)
            {
                output.Write(memory.Span);
            }
        }
    }

    public struct Message
    {

        public int MsgId { get; set; } = 0;

        public bool Ziped { get; set; } = false;

        public ReadOnlySequence<byte> Payload { get; }

        public Message(ReadOnlySequence<byte> payload)
        {
            Payload = payload;
        }

        public Message(int msgId, byte[] payload) 
        {
            int len = payload.Length;
            if (len >= 512)
            {
                Ziped = true;
                payload = MsgDecoder.CompressGZip(payload);
                //LOGGER.Debug($"msg:{msg.MsgId} zip before:{len}, after:{msgData.Length}");
            }
            Payload = new ReadOnlySequence<byte>(payload);
            MsgId = msgId;
        }

        public static Message Create(int msgId, byte[] payload)
        {
            return new Message(msgId, payload);
        }

    }
}
