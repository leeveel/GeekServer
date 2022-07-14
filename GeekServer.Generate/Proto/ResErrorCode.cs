//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject]
	public class ResErrorCode : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = 111005;
		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 0:表示无错误
        /// </summary>
        [Key(1)]
        public long ErrCode { get; set; }
        /// <summary>
        /// 错误描述（不为0时有效）
        /// </summary>
        [Key(2)]
        public string Desc { get; set; }
	}
}
