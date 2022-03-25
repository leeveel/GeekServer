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

            EntityMgr.Type2ID = EntityID.GetEntityIdFromType;
            EntityMgr.ID2Type = EntityID.GetEntityTypeFromID;

            RegisterComps();
            RobotManager.Start();

            Console.ReadLine();
        }


        private static void RegisterComps()
        {
            CompSetting.Singleton.RegistComp<RoleComp>((int)EntityType.Role, false);
            CompSetting.Singleton.RegistComp<NetComp>((int)EntityType.Role, false);
            CompSetting.Singleton.RegistComp<LoginComp>((int)EntityType.Role, false);
            CompSetting.Singleton.RegistComp<BagComp>((int)EntityType.Role, false);

            CompSetting.Singleton.SetIfEntityCompShareActor((int)EntityType.Role, true);
        }

    }
}
