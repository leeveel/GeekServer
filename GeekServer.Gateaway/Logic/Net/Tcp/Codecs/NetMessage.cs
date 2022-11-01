using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Gateaway.Net.Tcp
{
    public class NetMessage
    {
        public ReadOnlySequence<byte> Payload { get; } = default;

        public NetMessage(ReadOnlySequence<byte> payload)
        {
            Payload = payload;
        }

        public NetMessage(int msgId, byte[] msgData)
        {
            this.MsgId = msgId;
            MsgRaw = msgData;
        }

        public int MsgId;
        public byte[] MsgRaw;
    }
}
