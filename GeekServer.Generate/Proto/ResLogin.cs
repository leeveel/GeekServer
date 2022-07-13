//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject]
	public class ResLogin : Geek.Server.BaseMessage
	{
		[IgnoreMember]
		public const int Sid = 111002;
		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 登陆结果，0成功，其他时候为错误码
        /// </summary>
        [Key(1)]
        public int Code { get; set; }
        [Key(2)]
        public UserInfo UserInfo { get; set; }
	}
}
