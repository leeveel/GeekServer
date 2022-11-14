//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ResRouterMsg : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = 1803439793;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public bool Result { get; set; }
	}
}
