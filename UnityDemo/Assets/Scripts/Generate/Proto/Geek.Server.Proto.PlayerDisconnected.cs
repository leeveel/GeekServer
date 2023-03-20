//auto generated, do not modify it

using Protocol;
using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class PlayerDisconnected : Message
	{
		[IgnoreMember]
		public const int Sid = 1739074562;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 网关网络节点
        /// </summary>
        public int GateNodeId { get; set; }
	}
}
