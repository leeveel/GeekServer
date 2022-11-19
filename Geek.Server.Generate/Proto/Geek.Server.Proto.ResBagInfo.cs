//auto generated, do not modify it

using System.Collections.Generic;
using Geek.Server.Core.Net.Messages;
using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ResBagInfo : Geek.Server.Core.Net.Messages.Message
	{
		[IgnoreMember]
		public const int Sid = -1872884227;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public Dictionary<int, long> ItemDic { get; set; } = new Dictionary<int, long>();
	}
}
