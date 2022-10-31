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
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqBagInfo>(-399658839);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResBagInfo>(-768070425);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqComposePet>(-1888378530);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResComposePet>(-1498495527);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqUseItem>(1478385002);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqSellItem>(-1236539504);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResItemChange>(1593491631);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqRouterMsg>(-520770015);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResRouterMsg>(1063387717);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqDisconnectClient>(1739680047);
			settings.RegisterType<Geek.Server.Proto.A, Geek.Server.Proto.A>(-1878353591);
			settings.RegisterType<Geek.Server.Proto.A, Geek.Server.Proto.B>(14791156);
			settings.RegisterType<Geek.Server.Proto.UserInfo, Geek.Server.Proto.UserInfo>(724520320);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqLogin>(932720150);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResLogin>(2003930237);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResLevelUp>(1405557910);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.HearBeat>(1771311297);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResErrorCode>(-138811813);
        }
	}
}
