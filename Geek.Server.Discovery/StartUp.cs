using Geek.Server.Core.Net.Rpc;
using Geek.Server.Discovery.Logic;
using NLog;
using NLog.Config;
using PolymorphicMessagePack;

namespace Geek.Server.Discovery
{
    internal class StartUp
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static async Task Enter()
        {
            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/NLog.config");
                LogManager.AutoShutdown = false;
                Settings.Load<BaseSetting>("Configs/discovery_config.json", ServerType.Discovery);

                PolymorphicResolver.Instance.Init();

                Log.Info("开始启动..."); 

                NamingService.Instance.AddSelf();

                await RpcServer.Start(Settings.Ins.RpcPort);

                Settings.Ins.AppRunning = true;

                Log.Info("启动完成...");
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
    }
}
