//auto generated, do not modify it

using Geek.Client;

namespace Geek.Client.Proto
{
	public class SClassFactory
	{
		///<summary>通过msgId构造msg</summary>
		public static Serializable Create(int sid)
		{
			switch(sid)
			{
				case 112001: return new Geek.Server.Proto.ReqBagInfo();
				case 112002: return new Geek.Server.Proto.ResBagInfo();
				case 112003: return new Geek.Server.Proto.ReqUseItem();
				case 112004: return new Geek.Server.Proto.ReqSellItem();
				case 112005: return new Geek.Server.Proto.ResItemChange();
				case 111100: return new Geek.Server.Proto.UserInfo();
				case 111101: return new Geek.Server.Proto.ReqLogin();
				case 111102: return new Geek.Server.Proto.ResLogin();
				case 111103: return new Geek.Server.Proto.ResLevelUp();
				case 111104: return new Geek.Server.Proto.HearBeat();
				case 111105: return new Geek.Server.Proto.ResErrorCode();
				default: return default;
			}
		}
		
		public static T Create<T>(int sid) where T : Serializable
		{
			return (T)Create(sid);
		}

	}
}