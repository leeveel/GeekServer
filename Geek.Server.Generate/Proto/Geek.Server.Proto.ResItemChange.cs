//auto generated, do not modify it

using System.Collections.Generic;
using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ResItemChange : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = 901279609;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 变化的道具
        /// </summary>
        public Dictionary<int, long> ItemDic { get; set; }
	}
}
