//auto generated, do not modify it

using Geek.Server.Core.Net.Messages;
using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ResLevelUp : Geek.Server.Core.Net.Messages.Message
	{
		[IgnoreMember]
		public const int Sid = 1587576546;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 玩家等级
        /// </summary>
        public int Level { get; set; }
	}
}
