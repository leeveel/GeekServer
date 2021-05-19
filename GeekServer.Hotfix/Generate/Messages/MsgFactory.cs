//auto generated, do not modify it

namespace Geek.Server.Message
{
	public class MsgFactory
	{
		///<summary>通过msgId构造msg</summary>
		public static BaseMessage Create(int msgId)
		{
			switch(msgId)
			{
				//玩家快照
				case 101201: return new Geek.Server.Message.Login.ReqLogin();
				case 101202: return new Geek.Server.Message.Login.ReqReLogin();
				case 101101: return new Geek.Server.Message.Login.ResLogin();
				case 101102: return new Geek.Server.Message.Login.ResReLogin();
				case 101303: return new Geek.Server.Message.Login.HearBeat();
				case 101103: return new Geek.Server.Message.Login.ResPrompt();
				case 101104: return new Geek.Server.Message.Login.ResUnlockScreen();
				
				//举例各种结构写法
				case 111101: return new Geek.Server.Message.Sample.ReqTest();
				
				default: return default;
			}
		}
		
		public static T Create<T>(int msgId) where T : BaseMessage
		{
			return (T)Create(msgId);
		}
	}
}