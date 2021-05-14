namespace Geek.Server
{
    public interface IMessage
    {
        int GetMsgId();

        byte[] Serialize();

        void Deserialize(byte[] data);
    }
}