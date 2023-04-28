//auto generated, do not modify it

using Protocol;
using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ResInnerConnectGate : Message
	{
		[IgnoreMember]
		public const int Sid = 1306001561;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public bool IsSuccess { get; set; }
        public List<long> ClientIds { get; set; } //当前逻辑服的客户端id ，当逻辑服断线重连的时候，需要同步此数据给逻辑服
	}
}
