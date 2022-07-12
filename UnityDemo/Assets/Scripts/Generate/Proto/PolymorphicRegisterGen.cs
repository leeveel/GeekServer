namespace Geek.Server.Proto
{
	public partial class PolymorphicRegister
	{
	    static PolymorphicRegister()
        {
            System.Console.WriteLine("***PolymorphicRegister Init***");
			Init();
            Register();
        }

		public static void Register()
        {
			settings.RegisterType<Geek.Server.BaseMessage, Geek.Server.Proto.ReqBagInfo>(112001);
			settings.RegisterType<Geek.Server.BaseMessage, Geek.Server.Proto.ResBagInfo>(112002);
			settings.RegisterType<Geek.Server.BaseMessage, Geek.Server.Proto.ReqUseItem>(112003);
			settings.RegisterType<Geek.Server.BaseMessage, Geek.Server.Proto.ReqSellItem>(112004);
			settings.RegisterType<Geek.Server.BaseMessage, Geek.Server.Proto.ResItemChange>(112005);
			settings.RegisterType<Geek.Server.BaseMessage, Geek.Server.Proto.ReqLogin>(111001);
			settings.RegisterType<Geek.Server.BaseMessage, Geek.Server.Proto.ResLogin>(111002);
			settings.RegisterType<Geek.Server.BaseMessage, Geek.Server.Proto.ResLevelUp>(111003);
			settings.RegisterType<Geek.Server.BaseMessage, Geek.Server.Proto.HearBeat>(111004);
			settings.RegisterType<Geek.Server.BaseMessage, Geek.Server.Proto.ResErrorCode>(111005);
        }
	}
}
