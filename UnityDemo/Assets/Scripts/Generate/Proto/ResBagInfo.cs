//auto generated, do not modify it

using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject]
	public class ResBagInfo : Geek.Server.BaseMessage
	{
		[IgnoreMember]
		public const int Sid = 112002;
		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

		[Key(1)]
		public Dictionary<int, long> ItemDic { get; set; } = new Dictionary<int, long>();
	}
}
