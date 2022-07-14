//auto generated, do not modify it

using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject]
	public class ResItemChange : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = 112005;
		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

		/// <summary>
		/// 变化的道具
		/// </summary>
		[Key(1)]
		public Dictionary<int, long> ItemDic { get; set; }
	}
}
