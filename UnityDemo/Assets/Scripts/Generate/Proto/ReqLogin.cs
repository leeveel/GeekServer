//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject]
	public class ReqLogin : Geek.Server.BaseMessage
	{
		[IgnoreMember]
		public const int Sid = 111001;
		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

		[Key(1)]
        public string UserName { get; set; }
		[Key(2)]
        public string Platform { get; set; }
		[Key(3)]
        public int SdkType { get; set; }
		[Key(4)]
        public string SdkToken { get; set; }
		[Key(5)]
		public string Device { get; set; }
	}
}
