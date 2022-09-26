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
            settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqBagInfo>(-1612046553);
            settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResBagInfo>(-1516249246);
            settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqUseItem>(-693612441);
            settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqSellItem>(-1914477348);
            settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResItemChange>(-202674947);
            settings.RegisterType<Geek.Server.Proto.A, Geek.Server.Proto.A>(1544071388);
            settings.RegisterType<Geek.Server.Proto.A, Geek.Server.Proto.B>(1070961112);
            settings.RegisterType<Geek.Server.Proto.UserInfo, Geek.Server.Proto.UserInfo>(2003858448);
            settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ReqLogin>(205475772);
            settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResLogin>(2039746454);
            settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResLevelUp>(-1820231335);
            settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.HearBeat>(-1620014636);
            settings.RegisterType<Geek.Server.Message, Geek.Server.Proto.ResErrorCode>(-488702425);
        }
    }
}
