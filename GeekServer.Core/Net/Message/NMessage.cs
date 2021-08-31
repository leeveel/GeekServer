namespace Geek.Server
{
    public class NMessage
    {
        public byte[] Data { get; set; }
        public int MsgId { get; set; }

        public static NMessage Create(int msgId, byte[] data)
        {
            NMessage msg = new NMessage
            {
                MsgId = msgId,
                Data = data
            };
            return msg;
        }

    }
}
