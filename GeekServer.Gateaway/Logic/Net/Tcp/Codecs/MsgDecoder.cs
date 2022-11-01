
using Microsoft.AspNetCore.Connections;
using System;
using System.Buffers;
using System.IO;

namespace Geek.Server.Gateaway.Net.Tcp
{
    public static class MsgDecoder
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public const string LAST_RECV_ORDER = "LAST_RECV_ORDER";

        public const string LAST_RECV_TIME = "LAST_RECV_TIME";

        /// 从客户端接收的包大小最大值（单位：字节 1M）
        const int MAX_RECV_SIZE = 1024 * 1024;
        public static void Decode(ConnectionContext context, ref NetMessage msg)
        {
            var reader = new SequenceReader<byte>(msg.Payload);

            int msgLen = (int)msg.Payload.Length;  //4
            if (!CheckMsgLen(msgLen))
            {
                context.Abort();
                return;
            }

            reader.TryReadBigEndian(out long time); //8
            if (!CheckTime(context, time))
            {
                context.Abort();
                return;
            }

            reader.TryReadBigEndian(out int order);  //4
            if (!CheckMagicNumber(context, order, msgLen))
            {
                context.Abort();
                return;
            }

            reader.TryReadBigEndian(out int msgId);  //4
            msg.MsgId = msgId;

            msg.MsgRaw = reader.UnreadSequence.ToArray();
            return;
        }


        public static bool CheckMagicNumber(ConnectionContext context, int order, int msgLen)
        {
            order ^= (0x1234 << 8);
            order ^= msgLen + 4;
            context.Items.TryGetValue(LAST_RECV_ORDER, out object objOrder);
            if (objOrder != null)
            {
                int lastOrder = (int)objOrder;
                if (order != lastOrder + 1)
                {
                    LOGGER.Error("包序列出错, order=" + order + ", lastOrder=" + lastOrder);
                    return false;
                }
            }
            context.Items[LAST_RECV_ORDER] = order;
            return true;
        }

        /// <summary>
        /// 检查消息长度是否合法
        /// </summary>
        /// <param name="msgLen"></param>
        /// <returns></returns>
        public static bool CheckMsgLen(int msgLen)
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
        public static bool CheckTime(ConnectionContext context, long time)
        {
            context.Items.TryGetValue(LAST_RECV_TIME, out object objTime);
            if (objTime != null)
            {
                long lastTime = (long)objTime;
                if (lastTime > time)
                {
                    LOGGER.Error("时间戳出错，time=" + time + ", lastTime=" + lastTime + " " + context);
                    return false;
                }
            }
            context.Items[LAST_RECV_TIME] = time;
            return true;
        }

    }
}
