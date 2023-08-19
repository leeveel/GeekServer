//auto generated, do not modify it

using MessagePack;

namespace Geek.Server.Proto
{
	[MessagePackObject(true)]
	public class ResPrompt : Message
	{
		[IgnoreMember]
		public const int Sid = 537499886;

		[IgnoreMember]
		public const int MsgID = Sid;
		[IgnoreMember]
		public override int MsgId => MsgID;

        ///<summary>提示信息类型（1Tip提示，2跑马灯，3插队跑马灯，4弹窗，5弹窗回到登陆，6弹窗退出游戏）</summary>
		public int Type { get; set; }
        ///<summary>提示内容</summary>
        public string Content { get; set; }
	}
}
