//auto generated, do not modify it

using Protocol;
using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqDisconnectClient : Message
	{
		[IgnoreMember]
		public const int Sid = -1408529503;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public long NetId { get; set; }
	}
}