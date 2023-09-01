using Geek.Server.Core.Net.Rpc;
using NLog;
using NLog.Config;
using PolymorphicMessagePack;

namespace Server.Discovery
{
    internal class StartUp
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static async Task Enter()
        {
            try
            {
                var flag = Start();
                if (!flag) return; //启动服务器失败

                PolymorphicResolver.Instance.Init();

                Log.Info("开始启动...");
                await RpcServer.Start(Settings.Ins.RpcPort);

                Settings.Ins.AppRunning = true;
                await Settings.Ins.AppExitToken;
            }
            catch (Exception e)
            {
                Console.WriteLine($"服务器执行异常，e:{e}");
                Log.Fatal(e);
            }

            Console.WriteLine($"退出服务器开始");
            await RpcServer.Stop();
            Console.WriteLine($"退出服务器成功");
        }

        private static bool Start()
        {
            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/NLog.config");
                LogManager.AutoShutdown = false;
                Settings.Load<BaseSetting>("Configs/discovery_config.json", ServerType.Discovery);
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"启动服务器失败,异常:{e}");
                return false;
            }
        }
    }
}
