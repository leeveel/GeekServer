//auto generated, do not modify it

namespace Geek.Client.Message
{
	public class MsgFactory
	{
		///<summary>通过msgId构造msg</summary>
		public static BaseMessage Create(int msgId)
		{
			switch(msgId)
			{
				//背包
				case 112001: return new Geek.Client.Message.Bag.ReqBagInfo();
				case 112002: return new Geek.Client.Message.Bag.ResBagInfo();
				case 112003: return new Geek.Client.Message.Bag.ReqUseItem();
				case 112004: return new Geek.Client.Message.Bag.ReqSellItem();
				case 112005: return new Geek.Client.Message.Bag.ResItemChange();
				
				//登陆
				case 111001: return new Geek.Client.Message.Login.ReqLogin();
				case 111002: return new Geek.Client.Message.Login.ResLogin();
				case 111003: return new Geek.Client.Message.Login.ResLevelUp();
				case 111004: return new Geek.Client.Message.Login.ResNotice();
				case 111005: return new Geek.Client.Message.Login.ReqChangeName();
				case 111006: return new Geek.Client.Message.Login.ResChangeName();
				case 111007: return new Geek.Client.Message.Login.HearBeat();
				case 111008: return new Geek.Client.Message.Login.ResErrorCode();
				
				//举例各种结构写法
				case 111101: return new Geek.Client.Message.Sample.ReqTest();
				
				default: return default;
			}
		}
		
		public static T Create<T>(int msgId) where T : BaseMessage
		{
			return (T)Create(msgId);
		}
	}
}