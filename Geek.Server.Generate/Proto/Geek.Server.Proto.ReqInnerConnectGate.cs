//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqInnerConnectGate : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = -1857713043;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public int NodeId { get; set; }
	}
}