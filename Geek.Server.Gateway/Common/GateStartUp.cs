using NLog.LayoutRenderers;
using Geek.Server.Core.Extensions;
using Geek.Server.GatewayKcp;
using PolymorphicMessagePack;
using NLog;

namespace Geek.Server.Gateway.Common
{
    internal class GateStartUp
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static async Task Enter()
        {
            try
            {
                var flag = Start();
                if (!flag) return; //启动服务器失败

                Log.Info("进入游戏主循环...");
                Console.WriteLine("***进入游戏主循环***");

                await GateServer.Instance.Start();

                Settings.LauchTime = DateTime.Now;
                Settings.AppRunning = true;

                await Settings.AppExitToken;
                //try
                //{
                //    while (!Settings.AppExitToken.IsCancellationRequested)
                //    {
                //        GC.Collect();
                //        await Task.Delay(TimeSpan.FromMinutes(10), Settings.AppExitToken);
                //    }
                //}
                //catch
                //{
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine($"服务器执行异常，e:{e}");
                Log.Fatal(e);
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
                LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/gate_log.config");
                LogManager.AutoShutdown = false;

                PolymorphicResolver.Instance.Init();

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
