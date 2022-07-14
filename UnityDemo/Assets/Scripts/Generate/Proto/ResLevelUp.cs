//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject]
	public class ResLevelUp : Geek.Server.Message
	{
		[IgnoreMember]
		public const int Sid = 111003;
		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 玩家等级
        /// </summary>
        [Key(1)]
        public int Level { get; set; }
	}
}
