//auto generated, do not modify it

using Protocol;
using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqClientChannelActive : Message
	{
		[IgnoreMember]
		public const int Sid = -1734875143;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public string Address { get; set; }
	}
}
