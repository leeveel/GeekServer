//auto generated, do not modify it

using Protocol;
using MessagePack;
using System.Collections.Generic;
using Geek.Server.Core.Net.BaseHandler;

namespace Geek.Server.Proto
{
    [MessagePackObject(true)]
	public class ResItemChange : Message
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
