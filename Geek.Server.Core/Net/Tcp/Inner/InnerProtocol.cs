using System;
using System.Buffers;
using Bedrock.Framework;
using Bedrock.Framework.Protocols;
using Geek.Server.Core.Utils;

namespace Geek.Server.Core.Net.Tcp.Inner
{
    public class InnerProtocol : IProtocal<NetMessage>
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        const int MAX_RECV_SIZE = 1024 * 1024 * 2;

        private bool useRawMsgData;
        public InnerProtocol(bool useRawMsgData) { this.useRawMsgData = useRawMsgData; }
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out NetMessage message)
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
            if (useRawMsgData)
            {
                message = new NetMessage { MsgId = msgId, NetId = netId, MsgRaw = payload.ToArray() };
            }
            else
            {
                message = new NetMessage { MsgId = msgId, NetId = netId, Msg = MessagePack.MessagePackSerializer.Deserialize<Message>(payload) };
            }

            consumed = payload.End;
            examined = consumed;
            return true;
        }

        public void WriteMessage(NetMessage nmsg, IBufferWriter<byte> output)
        {
            byte[] bytes = nmsg.Serialize();
            int len = 16 + bytes.Length; //len(4) + clientConnId(8) + msgid(4)
            var span = output.GetSpan(len);
            int offset = 0;
            span.WriteInt(len, ref offset);
            span.WriteLong(nmsg.NetId, ref offset);
            span.WriteInt(nmsg.MsgId, ref offset);
            span.WriteBytesWithoutLength(bytes, ref offset);
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
