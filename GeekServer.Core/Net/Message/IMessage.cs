namespace Geek.Server
{
    public interface IMessage
    {
        /// <summary>
        /// 每次请求的UniqueId
        /// GeekServer遵从请求必有响应的原则
        /// 1v1：res消息的uniId应该等于req的uniId
        /// 1v多：最后一条res消息的uniId应该等于req的uniId
        /// 出现错误的时候：服务器也应该推送一条错误消息（ErrorMsg）其UniID应等于req的uniId
        /// 目的：1.方便客户端做await逻辑 2.解锁屏幕 3.断线重连消息id比对 (当然你也可以使用你自己的规则)
        /// </summary>
        int UniId { get; set; }

        int GetMsgId();

        byte[] Serialize();

        void Deserialize(byte[] data);
    }
}