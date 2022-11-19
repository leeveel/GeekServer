//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqConnectGate : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = -679570763;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public int ServerId { get; set; }
	}
}
