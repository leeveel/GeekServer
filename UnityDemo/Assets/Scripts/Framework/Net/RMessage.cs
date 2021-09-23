namespace Geek.Client
{
    public class RMessage
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public int MsgId { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        public int RetCode { get; set; }

        /// <summary>
        /// 字节流数据
        /// </summary>
        public byte[] ByteContent { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public BaseMessage Msg { get; set; }
    }
}