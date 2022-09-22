//auto generated, do not modify it

using System;
namespace Geek.Server.Proto
{
	public class MsgFactory
	{
		private static readonly System.Collections.Generic.Dictionary<int, Type> lookup;

        static MsgFactory()
        {
            lookup = new System.Collections.Generic.Dictionary<int, Type>(14)
            {
			    { -1612046553, typeof(Geek.Server.Proto.ReqBagInfo) },
			    { -1516249246, typeof(Geek.Server.Proto.ResBagInfo) },
			    { -693612441, typeof(Geek.Server.Proto.ReqUseItem) },
			    { -1914477348, typeof(Geek.Server.Proto.ReqSellItem) },
			    { -202674947, typeof(Geek.Server.Proto.ResItemChange) },
			    { -1657659890, typeof(Geek.Server.Proto.TestStruct) },
			    { 1544071388, typeof(Geek.Server.Proto.A) },
			    { 1070961112, typeof(Geek.Server.Proto.B) },
			    { 2003858448, typeof(Geek.Server.Proto.UserInfo) },
			    { 205475772, typeof(Geek.Server.Proto.ReqLogin) },
			    { 2039746454, typeof(Geek.Server.Proto.ResLogin) },
			    { -1820231335, typeof(Geek.Server.Proto.ResLevelUp) },
			    { -1620014636, typeof(Geek.Server.Proto.HearBeat) },
			    { -488702425, typeof(Geek.Server.Proto.ResErrorCode) },
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
