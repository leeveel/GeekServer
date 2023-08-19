//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class HearBeat : Message
	{
		[IgnoreMember]
		public const int Sid = 1575482382;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 当前时间
        /// </summary>
        public long TimeTick { get; set; }
	}
}
