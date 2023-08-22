//auto generated, do not modify it

using MessagePack;
using System.Collections.Generic;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ReqLogin : Message
	{
		[IgnoreMember]
		public const int Sid = 1267074761;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        public string UserName { get; set; }
        public string Platform { get; set; }
        public int SdkType { get; set; }
        public string SdkToken { get; set; }
        public string Device { get; set; }
        /// <summary>
        /// 连接标示，避免自己顶自己的号,客户端每次启动游戏生成一次/或者每个设备一个
        /// </summary>
        public string Sign { get; set; } 
	}
}
