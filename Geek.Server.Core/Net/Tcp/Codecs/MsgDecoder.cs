using System.Buffers;
using Geek.Server.Core.Hotfix;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Core.Net.Tcp.Codecs
{
    public static class MsgDecoder
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public const string LAST_RECV_ORDER = "LAST_RECV_ORDER";

        public const string LAST_RECV_TIME = "LAST_RECV_TIME";

        /// 从客户端接收的包大小最大值（单位：字节 1M）
        const int MAX_RECV_SIZE = 1024 * 1024;

        public static Message ClientDecode(NMessage message)
        {
            var reader = new SequenceReader<byte>(message.Payload);

            //数据包长度+消息ID=两个int=8位
            if (message.Payload.Length < 4)
                return null;

            //消息id
            reader.TryReadBigEndian(out int msgId);

            var msgType = HotfixMgr.GetMsgType(msgId);
            if (msgType == null)
            {
                LOGGER.Error("消息ID:{} 找不到对应的Msg.", msgId);
                return null;
            }
            var msg = MessagePack.MessagePackSerializer.Deserialize<Message>(reader.UnreadSequence);
            if (msg.MsgId != msgId)
            {
                LOGGER.Error("后台解析消息失败，注册消息id和消息无法对应.real:{0}, register:{1}", msg.MsgId, msgId);
                return null;
            }
            return msg;
        }

        public static Message Decode(ConnectionContext context, NMessage msg)
        {
            var reader = new SequenceReader<byte>(msg.Payload);

            int msgLen = (int)msg.Payload.Length;  //4
            if (!CheckMsgLen(msgLen))
            {
                context.Abort();
                return null;
            }

            reader.TryReadBigEndian(out long time); //8
            if (!CheckTime(context, time))
            {
                context.Abort();
                return null;
            }

            reader.TryReadBigEndian(out int order);  //4
            if (!CheckMagicNumber(context, order, msgLen))
            {
                context.Abort();
                return null;
            }

            reader.TryReadBigEndian(out int msgId);  //4

            var msgType = HotfixMgr.GetMsgType(msgId);
            if (msgType == null)
            {
                LOGGER.Error("消息ID:{} 找不到对应的Msg.", msgId);
                return null;
            }

            var protoMsg = MessagePack.MessagePackSerializer.Deserialize<Message>(reader.UnreadSequence);
            if (protoMsg.MsgId != msgId)
            {
                LOGGER.Error("后台解析消息失败，注册消息id和消息无法对应.real:{0}, register:{1}", protoMsg.MsgId, msgId);
                return null;
            }
            return protoMsg;
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
