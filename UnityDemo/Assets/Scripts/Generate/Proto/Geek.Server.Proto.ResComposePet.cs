//auto generated, do not modify it

using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ResComposePet : Message
	{
		[IgnoreMember]
		public const int Sid = 750865816;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        /// <summary>
        /// 合成宠物的Id
        /// </summary>
        public int PetId { get; set; }
	}
}
