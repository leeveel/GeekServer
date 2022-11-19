//auto generated, do not modify it

using System.Collections.Generic;
using Geek.Server.Core.Net.Messages;
using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqBagInfo : Geek.Server.Core.Net.Messages.Message
	{
		[IgnoreMember]
		public const int Sid = 1435193915;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

	}
}
