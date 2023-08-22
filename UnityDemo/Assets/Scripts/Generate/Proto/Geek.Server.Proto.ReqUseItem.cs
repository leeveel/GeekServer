//auto generated, do not modify it

using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqUseItem : Message
	{
		[IgnoreMember]
		public const int Sid = 1686846581;

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
