using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace Geek.Server.Center.Logic
{
    internal class CenterStartUp
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public static async Task Enter()
        {
            try
            {
                var flag = Start();
                if (!flag) return; //启动服务器失败

                Log.Info("进入游戏主循环...");
                Console.WriteLine("***进入游戏主循环***");
                await RpcServer.Start(Settings.InsAs<CenterSetting>().RpcPort);
                await HttpServer.Start(Settings.HttpPort);
                Settings.LauchTime = DateTime.Now;
                Settings.AppRunning = true;

                TimeSpan delay = TimeSpan.FromSeconds(1);
                while (Settings.AppRunning)
                {
                    await Task.Delay(delay);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"服务器执行异常，e:{e}");
                Log.Fatal(e);
            }

            Console.WriteLine($"退出服务器开始");
            await RpcServer.Stop();
            await HttpServer.Stop();
            Console.WriteLine($"退出服务器成功");
        }

        private static bool Start()
        {
            try
            {
                Console.WriteLine("init NLog config...");
                LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/center_log.config");
                LogManager.AutoShutdown = false;
                Settings.Load<CenterSetting>("Configs/center_config.json", ServerType.Center);
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
