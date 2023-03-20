//auto generated, do not modify it

using Protocol;
using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ResConnectGate : Message
	{
		[IgnoreMember]
		public const int Sid = 2096149334;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 服务器id
        /// </summary>
        public int ServerId { get; set; }
        /// <summary>
        /// 节点id(单服结构时==ServerId)
        /// </summary>
        public long NodeId { get; set; }
	}
}
