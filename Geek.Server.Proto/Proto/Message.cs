using MessagePack;

//外部message定义，不要修改此类 
[MessagePackObject(true)]
public class Message
{
    /// <summary>
    /// 消息唯一id
    /// </summary>
    public int UniId { get; set; }
    [IgnoreMember]
    public virtual int MsgId { get; }
}
