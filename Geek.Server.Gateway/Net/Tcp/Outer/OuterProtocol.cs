using System.Buffers;
using Bedrock.Framework.Protocols;
using Geek.Server.Core.Net.Messages;
using Geek.Server.Core.Utils;

namespace Geek.Server.Gateway;

public class OuterProtocol : IProtocal<NetMessage>
{
    public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out NetMessage message)
    {
        var reader = new SequenceReader<byte>(input);
        //客户端传过来的length包含了长度自身（data: [length:byte[1,2,3,4]] ==> 则length=int 4 个字节+byte数组长度4=8）
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

    public void WriteMessage(NetMessage nmsg, IBufferWriter<byte> output)
    {
        byte[] bytes = nmsg.GetBytes();
        int len = 8 + bytes.Length;
        var span = output.GetSpan(len);
        int offset = 0;
        XBuffer.WriteInt(len, span, ref offset);
        XBuffer.WriteInt(nmsg.MsgId, span, ref offset);
        XBuffer.WriteBytesWithoutLength(bytes, span, ref offset);
        output.Advance(len);
    }

}
