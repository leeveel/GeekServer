﻿using System;
using System.Buffers;
using Bedrock.Framework.Protocols;
using Geek.Server.Core.Utils;
using MessagePack;
using Protocol;

namespace Geek.Server.TestPressure.Logic
{
    public class ClientProtocol : IProtocal<NetMessage>
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out NetMessage message)
        {
            message = default;
            var reader = new SequenceReader<byte>(input);

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
            var msgType = MsgFactory.GetType(msgId);
            if (msgType == null)
            {
                LOGGER.Error($"消息ID:{msgId} 找不到对应的Msg.");
            }
            else
            {
                message = new NetMessage { Msg = MessagePackSerializer.Deserialize<Message>(payload.Slice(4)), MsgId = msgId };
                if (message.MsgId != msgId)
                {
                    throw new ProtocalParseErrorException($"解析消息错误，注册消息id和消息无法对应.real:{message.MsgId}, register:{msgId}");
                }
            }
            return true;
        }

        private const int Magic = 0x1234;
        int count = 0;
        public void WriteMessage(NetMessage msg, IBufferWriter<byte> output)
        {
            //length + timestamp + magic + msgid
            var bytes = msg.Serialize();
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
        }
    }
}