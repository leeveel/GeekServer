//auto generated, do not modify it

using System;
namespace Geek.Server.Proto
{
	public class MsgFactory
	{
		private static readonly System.Collections.Generic.Dictionary<int, Type> lookup;

        static MsgFactory()
        {
            lookup = new System.Collections.Generic.Dictionary<int, Type>(17)
            {
			    { -399658839, typeof(Geek.Server.Proto.ReqBagInfo) },
			    { -768070425, typeof(Geek.Server.Proto.ResBagInfo) },
			    { -1888378530, typeof(Geek.Server.Proto.ReqComposePet) },
			    { -1498495527, typeof(Geek.Server.Proto.ResComposePet) },
			    { 1478385002, typeof(Geek.Server.Proto.ReqUseItem) },
			    { -1236539504, typeof(Geek.Server.Proto.ReqSellItem) },
			    { 1593491631, typeof(Geek.Server.Proto.ResItemChange) },
			    { -248186732, typeof(Geek.Server.Proto.TestStruct) },
			    { -1878353591, typeof(Geek.Server.Proto.A) },
			    { 14791156, typeof(Geek.Server.Proto.B) },
			    { 724520320, typeof(Geek.Server.Proto.UserInfo) },
			    { 932720150, typeof(Geek.Server.Proto.ReqLogin) },
			    { 2003930237, typeof(Geek.Server.Proto.ResLogin) },
			    { 1405557910, typeof(Geek.Server.Proto.ResLevelUp) },
			    { 1771311297, typeof(Geek.Server.Proto.HearBeat) },
			    { -138811813, typeof(Geek.Server.Proto.ResErrorCode) },
			    { -353424320, typeof(Geek.Server.Proto.ResPrompt) },
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
