//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ResInnerConnectGate : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = 1306001561;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public bool IsSuccess { get; set; }
	}
}
