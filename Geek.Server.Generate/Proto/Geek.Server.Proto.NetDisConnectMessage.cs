//auto generated, do not modify it

using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class NetDisConnectMessage : Message
	{
		[IgnoreMember]
		public const int Sid = 1599184588;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public NetCode Code { get; set; }
	}
}
