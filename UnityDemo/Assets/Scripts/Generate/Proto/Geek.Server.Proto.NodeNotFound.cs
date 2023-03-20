//auto generated, do not modify it

using Protocol;
using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class NodeNotFound : Message
	{
		[IgnoreMember]
		public const int Sid = -498188700;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public int NodeId { get; set; }
	}
}
