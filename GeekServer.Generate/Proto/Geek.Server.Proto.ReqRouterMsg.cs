//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqRouterMsg : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = -520770015;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public long TargetUid { get; set; }
	}
}
