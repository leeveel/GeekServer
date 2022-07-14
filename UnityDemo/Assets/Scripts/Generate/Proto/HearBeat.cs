//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject]
	public class HearBeat : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = 111004;
		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 当前时间
        /// </summary>
        [Key(1)]
        public long TimeTick { get; set; }
	}
}
