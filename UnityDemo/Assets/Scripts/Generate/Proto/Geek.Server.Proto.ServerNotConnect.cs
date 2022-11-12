//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ServerNotConnect : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = -37174277;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public long serverUid { get; set; }
	}
}
