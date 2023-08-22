//auto generated, do not modify it

using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqComposePet : Message
	{
		[IgnoreMember]
		public const int Sid = 225320501;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 碎片id
        /// </summary>
        public int FragmentId { get; set; }
	}
}
