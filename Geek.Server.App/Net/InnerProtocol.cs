using System;
using System.Buffers;
using System.Threading;
using Bedrock.Framework;
using Bedrock.Framework.Protocols;
using MessagePack;
using NLog;

namespace Geek.Server.App.Net
{
    public class InnerProtocol : IProtocal<Message>
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        const int MAX_RECV_SIZE = 1024 * 1024 * 20;

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out Message message)
        {
            message = default;
            var reader = new SequenceReader<byte>(input);

            if (!reader.TryReadBigEndian(out int msgLen))
            {
                consumed = input.End; //告诉read task，到这里为止还不满足一个消息的长度，继续等待更多数据
                return false;
            }

            if (!CheckMsgLen(msgLen))
            {
                throw new ProtocalParseErrorException("消息长度异常");
            }

            if (reader.Remaining < msgLen - 4)
            {
                consumed = input.End;
                return false;
            }

            reader.TryReadBigEndian(out long netId); // 8 
            reader.TryReadBigEndian(out int msgId);     //4  

            var payload = input.Slice(reader.Position, msgLen - 16);
            message = MessagePackSerializer.Deserialize<Message>(payload);
            message.SrcNetId = netId;

            consumed = payload.End;
            examined = consumed;
            return true;
        }

        public void WriteMessage(Message msg, IBufferWriter<byte> output)
        {
            var bytes = MessagePackSerializer.Serialize(msg);
            int len = 16 + bytes.Length;
            var span = output.GetSpan(len);
            int offset = 0;
            span.WriteInt(len, ref offset);
            span.WriteLong(msg.SrcNetId, ref offset);
            span.WriteInt(msg.MsgId, ref offset);
            bytes.CopyTo(span.Slice(16));
            output.Advance(len);
        }

        public bool CheckMsgLen(int msgLen)
        {
            //msglen(4)+cliengConnId(8)+msgId(4)=16位
            if (msgLen <= 12)//(消息长度已经被读取)
            {
                LOGGER.Error("从客户端接收的包大小异常:" + msgLen + ":至少大于12个字节");
                return false;
            }
            else if (msgLen > MAX_RECV_SIZE)
            {
                LOGGER.Warn("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值：" + MAX_RECV_SIZE / 1024 + "字节");
                return true;
            }
            return true;
        }
    }
}
