//auto generated, do not modify it

using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject]
	public class ReqSellItem : Geek.Server.BaseMessage
	{
		[IgnoreMember]
		public const int Sid = 112004;
		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

		/// <summary>
		/// 道具id
		/// </summary>
		[Key(1)]
		public int ItemId { get; set; }
	}
}
