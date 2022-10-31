//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqDisconnectClient : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = 1739680047;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public long TargetUid { get; set; }
	}
}
