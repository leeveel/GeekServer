using Geek.Server.App.Discovery;
using Geek.Server.App.Net.GatewayKcp;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Net.Kcp;
using Geek.Server.Core.Storage;
using Geek.Server.Proto;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using PolymorphicMessagePack;

namespace Geek.Server.App.Common
{
    internal class AppStartUp
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static async Task Enter()
        {
            try
            {
                var flag = Start();
                if (!flag) return; //启动服务器失败



                Log.Info($"launch embedded db...");
                GameDB.Init();
                GameDB.Open();

                Log.Info($"regist comps...");
                await CompRegister.Init();
                Log.Info($"load hotfix module");
                await HotfixMgr.LoadHotfixModule();

                Settings.InsAs<AppSetting>().ServerReady = true;

                _ = new AppDiscoveryClient().Start();

                var kcpServer = new KcpServer(Settings.Ins.InnerPort, KcpHander.OnMessage, KcpHander.OnChannelRemove, AppDiscoveryClient.Instance.GetServerInnerEndPoint);
                kcpServer.Start();

                Log.Info("进入游戏主循环...");
                Console.WriteLine("***进入游戏主循环***");
                Settings.Ins.AppRunning = true;

                await Settings.Ins.AppExitToken;

                kcpServer.Stop();
            }
            catch (Exception e)
            {
                var re = e.InnerException != null ? e.InnerException : e;
                Console.WriteLine($"服务器执行异常，e:{re}");
                Log.Fatal(re);
            }

            Console.WriteLine($"退出服务器开始");
            await HotfixMgr.Stop();
            Console.WriteLine($"退出服务器成功");
        }

        private static bool Start()
        {
            try
            {
                Settings.Load<AppSetting>("Configs/app_config.json", ServerType.Game);
                Console.WriteLine("init NLog config...");
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/NLog.config");
                LogManager.AutoShutdown = false;

                PolymorphicTypeMapper.Register(typeof(AppStartUp).Assembly); //app
                PolymorphicRegister.Load();
                PolymorphicResolver.Instance.Init();
                //GeekServerAppPolymorphicDBStateRegister.Load();

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
