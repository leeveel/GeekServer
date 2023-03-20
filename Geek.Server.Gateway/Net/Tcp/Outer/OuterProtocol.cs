﻿using System.Buffers;
using Bedrock.Framework;
using Bedrock.Framework.Protocols;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Utils;
using MessagePack;

namespace Geek.Server.Gateway;

public class OuterProtocol : IProtocal<NetMessage>
{
    static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

    public const string LAST_RECV_ORDER = "LAST_RECV_ORDER";

    long lastReviceTime = 0;
    int lastOrder = 0;
    const int MAX_RECV_SIZE = 1024 * 1024 * 2; /// 从客户端接收的包大小最大值（单位：字节 5M）

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

        var payload = input.Slice(reader.Position, msgLen - 4);

        reader.TryReadBigEndian(out long time); //8
        if (!CheckTime(time))
        {
            throw new ProtocalParseErrorException("消息接收时间错乱");
        }

        reader.TryReadBigEndian(out int order);  //4
        if (!CheckMagicNumber(order, msgLen))
        {
            throw new ProtocalParseErrorException("消息order错乱");
        }

        reader.TryReadBigEndian(out int msgId);  //4

        message = new NetMessage { MsgId = msgId, MsgRaw = payload.Slice(16).ToArray() };

        consumed = payload.End;
        examined = consumed;

        return true;
    }

    public void WriteMessage(NetMessage nmsg, IBufferWriter<byte> output)
    {
        byte[] bytes = nmsg.Serialize();
        int len = 8 + bytes.Length;
        var span = output.GetSpan(len);
        int offset = 0;
        span.WriteInt(len, ref offset);
        span.WriteInt(nmsg.MsgId, ref offset);
        span.WriteBytesWithoutLength(bytes, ref offset);
        output.Advance(len);
    }

    public bool CheckMagicNumber(int order, int msgLen)
    {
        order ^= (0x1234 << 8);
        order ^= msgLen;

        if (lastOrder != 0 && order != lastOrder + 1)
        {
            LOGGER.Error("包序列出错, order=" + order + ", lastOrder=" + lastOrder);
            return false;
        }
        lastOrder = order;
        return true;
    }

    /// <summary>
    /// 检查消息长度是否合法
    /// </summary>
    /// <param name="msgLen"></param>
    /// <returns></returns>
    public bool CheckMsgLen(int msgLen)
    {
        //消息长度+时间戳+magic+消息id+数据
        //4 + 8 + 4 + 4 + data
        if (msgLen <= 16)//(消息长度已经被读取)
        {
            LOGGER.Error("从客户端接收的包大小异常:" + msgLen + ":至少16个字节");
            return false;
        }
        else if (msgLen > MAX_RECV_SIZE)
        {
            LOGGER.Error("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值：" + MAX_RECV_SIZE / 1024 + "字节");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 时间戳检查(可以防止客户端游戏过程中修改时间)
    /// </summary>
    /// <param name="context"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool CheckTime(long time)
    {
        if (lastReviceTime > time)
        {
            LOGGER.Error("时间戳出错，time=" + time + ", lastTime=" + lastReviceTime);
            return false;
        }
        lastReviceTime = time;
        return true;
    }

}
