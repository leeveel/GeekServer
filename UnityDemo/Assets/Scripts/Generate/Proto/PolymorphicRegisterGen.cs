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
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqBagInfo>(112001);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResBagInfo>(112002);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqUseItem>(112003);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqSellItem>(112004);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResItemChange>(112005);
			settings.RegisterType<Geek.Server.Proto.A, Geek.Server.Proto.A>(111111);
			settings.RegisterType<Geek.Server.Proto.A, Geek.Server.Proto.B>(111112);
			settings.RegisterType<Geek.Server.Proto.UserInfo, Geek.Server.Proto.UserInfo>(111000);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqLogin>(111001);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResLogin>(111002);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResLevelUp>(111003);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.HearBeat>(111004);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResErrorCode>(111005);
        }
	}
}
