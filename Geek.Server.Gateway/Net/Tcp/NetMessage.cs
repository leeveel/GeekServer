using MessagePack;
using System.Buffers;

namespace Geek.Server.Gateway.Net.Tcp
{
    public class NetMessage
    {
        public int MsgId { get; set; } = 0;
        public Message Msg { get; set; }
        public byte[] MsgRaw { get; set; }
        public int MsgRawLength { get; set; } = -1;
        public long NetId { get; set; } = 0;

        private NetMessage() { }
        public NetMessage(int msgId, long netId, byte[] msgRaw, int msgRawLength)
        {
            MsgId = msgId;
            NetId = netId;
            MsgRaw = msgRaw;
            MsgRawLength = msgRawLength;
        }

        public NetMessage(Message msg, long netId)
        {
            Msg = msg;
            NetId = netId;
            MsgId = Msg.MsgId;
        }

        public Span<byte> Serialize()
        {
            return MsgRaw != null ? new Span<byte>(MsgRaw).Slice(0, MsgRawLength) : MessagePackSerializer.Serialize(Msg);
        }

        public Message Deserialize()
        {
            Msg = MessagePackSerializer.Deserialize<Message>(new ReadOnlySequence<byte>(MsgRaw, 0, MsgRawLength));
            return Msg;
        }

        public void ReturnRawMenory()
        {
            if (MsgRaw != null)
                ArrayPool<byte>.Shared.Return(MsgRaw);
            MsgRaw = null;
            MsgRawLength = -1;
        }
    }
}
