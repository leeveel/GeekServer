
using Geek.Server.App.Net;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Center;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Hotfix;
using PolymorphicMessagePack;
using Geek.Server.Core.Storage;
using Geek.Server.Proto;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers; 
using Geek.Server.Core.Net.Kcp;
using Geek.Server.App.Net.GatewayKcp;
using Geek.Server.Core.Extensions;

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

                await Task.Run(async () =>
                {
                    //连接中心rpc
                    AppCenterRpcClient centerRpc = new AppCenterRpcClient(Settings.CenterUrl);
                    if (await centerRpc.Connect())
                    {
                        var getNode = () =>
                        {
                            return new ServerInfo
                            {
                                ServerId = Settings.ServerId,
                                Ip = Settings.LocalIp,
                                InnerIp = Settings.LocalIp,
                                TcpPort = Settings.TcpPort,
                                HttpPort = Settings.HttpPort,
                                Type = ServerType.Game
                            };
                        };

                        if (!await centerRpc.Register(getNode))
                            throw new Exception($"中心服注册失败... {JsonConvert.SerializeObject(getNode())}");

                        Log.Info($"launch embedded db...");
                        GameDB.Init();
                        GameDB.Open();

                        Log.Info($"regist comps...");
                        await CompRegister.Init();
                        Log.Info($"load hotfix module");
                        await HotfixMgr.LoadHotfixModule();

                        Settings.InsAs<AppSetting>().ServerReady = true;

                        //kcp test
                        _ = new KcpServer(Settings.TcpPort, KcpHander.OnMessage,KcpHander.OnChannelRemove).Start();
                    }
                });

                Log.Info("进入游戏主循环...");
                Console.WriteLine("***进入游戏主循环***");
                Settings.LauchTime = DateTime.Now;
                Settings.AppRunning = true;
                await Settings.AppExitToken;
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
                LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/app_log.config");
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
