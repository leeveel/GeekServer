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
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqBagInfo>(1435193915);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResBagInfo>(-1872884227);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqComposePet>(225320501);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResComposePet>(750865816);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqUseItem>(1686846581);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqSellItem>(-1395845865);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResItemChange>(901279609);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqConnectGate>(-679570763);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResConnectGate>(2096149334);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.NodeNotFound>(-498188700);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqInnerConnectGate>(-1857713043);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResInnerConnectGate>(1306001561);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.PlayerDisconnected>(1739074562);
			settings.RegisterType<Geek.Server.Proto.A, Geek.Server.Proto.A>(1250601847);
			settings.RegisterType<Geek.Server.Proto.A, Geek.Server.Proto.B>(-899515946);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqLogin>(1267074761);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResLogin>(785960738);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResLevelUp>(1587576546);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.HearBeat>(1575482382);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResErrorCode>(1179199001);
			settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResPrompt>(537499886);
        }
	}
}
