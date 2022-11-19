//auto generated, do not modify it

using System.Collections.Generic;
using Geek.Server.Core.Net.Messages;
using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ResComposePet : Geek.Server.Core.Net.Messages.Message
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
