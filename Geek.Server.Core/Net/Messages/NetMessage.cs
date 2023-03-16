using System.Buffers;

namespace Geek.Server
{
    public class NetMessage
    {
        public int MsgId { get; set; }
        public object Msg { get; set; }
        public NetMessage()
        {
        }

        public NetMessage(object msg)
        {
            this.Msg = msg;
        }

        public byte[] Serialize()
        {
            return MessagePack.MessagePackSerializer.Serialize(Msg);
        }
    }
}
