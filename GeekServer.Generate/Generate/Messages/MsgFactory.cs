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
				//背包
				case 112001: return new Geek.Server.Message.Bag.ReqBagInfo();
				case 112002: return new Geek.Server.Message.Bag.ResBagInfo();
				case 112003: return new Geek.Server.Message.Bag.ReqUseItem();
				case 112004: return new Geek.Server.Message.Bag.ReqSellItem();
				case 112005: return new Geek.Server.Message.Bag.ResItemChange();
				
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