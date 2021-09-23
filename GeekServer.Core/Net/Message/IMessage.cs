namespace Geek.Server
{
    public interface IMessage
    {
        /// <summary>
        /// 每次请求的UniqueId
        /// </summary>
        int UniId { get; set; }

        int GetMsgId();

        byte[] Serialize();

        void Deserialize(byte[] data);
    }
}