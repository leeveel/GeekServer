using PolymorphicMessagePack;

namespace Geek.Server.Gateway.Common
{
    internal class GateStartUp
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public static async Task Enter()
        {
            try
            {
                var flag = Start();
                if (!flag) return; //启动服务器失败

                LOGGER.Info("启动...");

                Settings.Ins.AppRunning = true;
                GateServer.Instance.Start(); 

                await Settings.Ins.AppExitToken;
            }
            catch (Exception e)
            {
                Console.WriteLine($"服务器执行异常，e:{e}");
                LOGGER.Fatal(e);
            }

            Console.WriteLine($"退出服务器开始");
            await GateServer.Instance.Stop();
            Console.WriteLine($"退出服务器成功");
        }

        private static bool Start()
        {
            try
            {
                Settings.Load<GateSettings>("Configs/gate_config.json", ServerType.Gate);
                Console.WriteLine("init NLog config...");
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/NLog.config");
                LogManager.AutoShutdown = false;

                PolymorphicResolver.Instance.Init();

                return true;
            }
            catch (Exception e)
            {
                LOGGER.Error($"启动服务器失败,异常:{e}");
                return false;
            }
        }
    }
}
