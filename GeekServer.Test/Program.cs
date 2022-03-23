using Geek.Server.Config;
using Geek.Server.Message;
using NLog;
using NLog.Config;
using System;

namespace Geek.Server.Test
{
    class Program
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Console.WriteLine("init NLog config...");
            LogManager.Configuration = new XmlLoggingConfiguration("Config/test_nlog.config");

            Console.WriteLine("init config...");
            Settings.Load("Config/server_config.json", ServerType.Game);
            Settings.Ins.AppRunning = true;
            Console.WriteLine("init config...");
            RobotSetting.Load("Config/test_config.json");

            Console.WriteLine("初始化消息工厂......");
            TcpHandlerFactory.InitHandler(typeof(RobotManager));

            ActorManager.ID_RULE = ActorID.ID_RULE;
            RegisterComps();
            RobotManager.Start();

            Console.ReadLine();
        }


        private static void RegisterComps()
        {
            ComponentMgr.Singleton.RegistComp<RoleComp>((int)ActorType.Role, true);
            ComponentMgr.Singleton.RegistComp<NetComp>((int)ActorType.Role, false);
            ComponentMgr.Singleton.RegistComp<LoginComp>((int)ActorType.Role, false);
            ComponentMgr.Singleton.RegistComp<BagComp>((int)ActorType.Role, false);
        }

    }
}
