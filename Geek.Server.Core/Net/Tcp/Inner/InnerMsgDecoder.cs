using Geek.Server.Core.Net.Messages;
using System.Buffers;

namespace Geek.Server.Core.Net.Tcp.Inner
{
    public static class InnerMsgDecoder
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 从客户端接收的包大小最大值（单位：字节 1M）
        /// </summary>
        const int MAX_RECV_SIZE = 1024 * 1024;

        public static void Decode(ref NetMessage msg)
        {
            var reader = new SequenceReader<byte>(msg.Payload);

            int msgLen = (int)msg.Payload.Length;  //4
            if (!CheckMsgLen(msgLen))
                return;

            reader.TryReadBigEndian(out long connId); // 8
            msg.ClientConnId = connId;

            reader.TryReadBigEndian(out int msgId);     //4
            msg.MsgId = msgId;

            msg.MsgRaw = reader.UnreadSequence.ToArray();
        }

        /// <summary>
        /// 检查消息长度是否合法
        /// </summary>
        /// <param name="msgLen"></param>
        /// <returns></returns>
        public static bool CheckMsgLen(int msgLen)
        {
            //msglen(4)+cliengConnId(8)+msgId(4)=16位
            if (msgLen <= 12)//(消息长度已经被读取)
            {
                LOGGER.Error("从客户端接收的包大小异常:" + msgLen + ":至少大于12个字节");
                return false;
            }
            else if (msgLen > MAX_RECV_SIZE)
            {
                LOGGER.Warn("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值：" + MAX_RECV_SIZE / 1024 + "字节");
                return true;
            }
            return true;
        }

    }
}
