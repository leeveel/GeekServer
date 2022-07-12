//auto generated, do not modify it

using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject]
	public class ReqBagInfo : Geek.Server.BaseMessage
	{
		[IgnoreMember]
		public const int Sid = 112001;
		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

	}
}
