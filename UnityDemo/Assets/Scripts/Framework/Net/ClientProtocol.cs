using System;
using System.Buffers;
using MessagePack;
using Bedrock.Framework.Protocols;
using Bedrock.Framework;
using Debug = UnityEngine.Debug;
using Protocol;

namespace Base.Net
{
    public class ClientProtocol : IProtocal<Message>
    {
        Func<int, Type> getMsgTypeFunc = null;

        public ClientProtocol(Func<int, Type> getMsgTypeFunc)
        {
            this.getMsgTypeFunc = getMsgTypeFunc;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out Message message)
        {
            message = default;
            var reader = new MessagePack.SequenceReader<byte>(input);

            if (!reader.TryReadBigEndian(out int length) || reader.Remaining < length - 4)
            {
                consumed = input.End; //告诉read task，到这里为止还不满足一个消息的长度，继续等待更多数据
                return false;
            }

            var payload = input.Slice(reader.Position, length - 4);
            if (payload.Length < 4)
                throw new ProtocalParseErrorException("消息长度不够");

            consumed = payload.End;
            examined = consumed;

            //消息id
            reader.TryReadBigEndian(out int msgId);
            var msgType = getMsgTypeFunc(msgId);
            if (msgType == null)
            {
                Debug.LogError($"消息ID:{msgId} 找不到对应的Msg.");
                //throw new ProtocalParseErrorException("不能发现消息对应类型");
            }
            else
            {
                message = MessagePackSerializer.Deserialize<Message>(payload.Slice(4));
#if UNITY_EDITOR
                Debug.Log("收到消息:" + MessagePackSerializer.SerializeToJson(message));
#endif
                if (message.MsgId != msgId)
                {
                    throw new ProtocalParseErrorException($"解析消息错误，注册消息id和消息无法对应.real:{message.MsgId}, register:{msgId}");
                }
            }
            return true;
        }

        private const int Magic = 0x1234;
        int count = 0;
        public void WriteMessage(Message msg, IBufferWriter<byte> output)
        {
            //length + timestamp + magic + msgid
            var bytes = MessagePack.MessagePackSerializer.Serialize(msg);
            int len = 4 + 8 + 4 + 4 + bytes.Length;
            var span = output.GetSpan(len);

            int magic = Magic + ++count;
            magic ^= Magic << 8;
            magic ^= len;

            int offset = 0;
            span.WriteInt(len, ref offset);
            span.WriteLong(DateTime.Now.Ticks, ref offset);
            span.WriteInt(magic, ref offset);
            span.WriteInt(msg.MsgId, ref offset);
            span.WriteBytesWithoutLength(bytes, ref offset);
            output.Advance(len);

#if UNITY_EDITOR
            Debug.Log("发送消息:" + MessagePackSerializer.ConvertToJson(bytes));
#endif
        }
    }

}
