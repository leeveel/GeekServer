//auto generated, do not modify it

using Protocol;
using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqInnerConnectGate : Message
	{
		[IgnoreMember]
		public const int Sid = -1857713043;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public int SelfNetId { get; set; }
	}
}
