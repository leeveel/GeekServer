using Geek.Server.Center.Logic;
using Geek.Server.Center.Web;
using Geek.Server.Core.Net.Http;
using Geek.Server.Core.Net.Rpc;
using PolymorphicMessagePack;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace Geek.Server.Center.Common
{
    internal class CenterStartUp
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static async Task Enter()
        {
            try
            {
                var flag = Start();
                if (!flag) return; //启动服务器失败

                PolymorphicResolver.Init();

                Log.Info("进入游戏主循环...");
                Console.WriteLine("***进入游戏主循环***");
                await WebServer.Start(Settings.InsAs<CenterSetting>().WebServerUrl);

                Settings.LauchTime = DateTime.Now;
                Settings.AppRunning = true;

                //服务节点添加自己
                ServiceManager.NamingService.Add(new Core.Center.NetNode
                {
                    NodeId = Settings.ServerId,
                    ServerId = Settings.ServerId,
                    Type = Core.Center.NodeType.Center,
                    RpcPort = Settings.RpcPort,
                });

                await RpcServer.Start(Settings.RpcPort);

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
            await WebServer.Stop();
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
