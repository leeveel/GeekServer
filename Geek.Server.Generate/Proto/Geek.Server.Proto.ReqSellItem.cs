//auto generated, do not modify it

using System.Collections.Generic;
using Geek.Server.Core.Net.Messages;
using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqSellItem : Geek.Server.Core.Net.Messages.Message
	{
		[IgnoreMember]
		public const int Sid = -1395845865;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 道具id
        /// </summary>
        public int ItemId { get; set; }
	}
}
