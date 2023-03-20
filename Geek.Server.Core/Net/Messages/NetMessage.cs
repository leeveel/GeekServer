using MessagePack;
using System.Buffers;

namespace Geek.Server
{
    public struct NetMessage
    {
        public int MsgId { get; set; } = 0;
        public Message Msg { get; set; } = null;
        public byte[] MsgRaw { get; set; } = default;
        public long NetId { get; set; } = 0;
        public NetMessage()
        {
        }

        public byte[] Serialize()
        {
            return MsgRaw != null ? MsgRaw : MessagePackSerializer.Serialize(Msg);
        }
    }
}
