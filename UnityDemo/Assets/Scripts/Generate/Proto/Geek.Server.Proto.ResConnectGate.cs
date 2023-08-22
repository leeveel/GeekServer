//auto generated, do not modify it

using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ResConnectGate : Message
	{
		[IgnoreMember]
		public const int Sid = 2096149334;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public bool Result { get; set; }
	}
}
