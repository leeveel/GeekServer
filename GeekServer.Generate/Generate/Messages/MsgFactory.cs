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
				//登陆
				case 111001: return new Geek.Server.Message.Login.ReqLogin();
				case 111002: return new Geek.Server.Message.Login.ResLogin();
				case 111003: return new Geek.Server.Message.Login.ResLevelUp();
				case 111004: return new Geek.Server.Message.Login.ResNotice();
				case 111005: return new Geek.Server.Message.Login.ReqChangeName();
				case 111006: return new Geek.Server.Message.Login.ResChangeName();
				case 101303: return new Geek.Server.Message.Login.HearBeat();
				
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