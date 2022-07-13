//auto generated, do not modify it

using System;
namespace Geek.Server.Proto
{
	public class MsgFactory
	{
		private static readonly System.Collections.Generic.Dictionary<int, Type> lookup;

        static MsgFactory()
        {
            lookup = new System.Collections.Generic.Dictionary<int, Type>(13)
            {
			    { 112001, typeof(Geek.Server.Proto.ReqBagInfo) },
			    { 112002, typeof(Geek.Server.Proto.ResBagInfo) },
			    { 112003, typeof(Geek.Server.Proto.ReqUseItem) },
			    { 112004, typeof(Geek.Server.Proto.ReqSellItem) },
			    { 112005, typeof(Geek.Server.Proto.ResItemChange) },
			    { 111111, typeof(Geek.Server.Proto.A) },
			    { 111112, typeof(Geek.Server.Proto.B) },
			    { 111000, typeof(Geek.Server.Proto.UserInfo) },
			    { 111001, typeof(Geek.Server.Proto.ReqLogin) },
			    { 111002, typeof(Geek.Server.Proto.ResLogin) },
			    { 111003, typeof(Geek.Server.Proto.ResLevelUp) },
			    { 111004, typeof(Geek.Server.Proto.HearBeat) },
			    { 111005, typeof(Geek.Server.Proto.ResErrorCode) },
            };
        }

        public static Type GetType(int msgId)
		{
			if (lookup.TryGetValue(msgId, out Type res))
				return res;
			else
				throw new Exception($"can not find msg type :{msgId}");
		}

	}
}
