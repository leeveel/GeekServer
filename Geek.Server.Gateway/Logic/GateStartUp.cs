using Geek.Server.Core.Center;
using Geek.Server.Gateway.Logic.Net;
using Geek.Server.Gateway.Logic.Net.Codecs;
using Geek.Server.Gateway.MessageHandler;
using Geek.Server.Proto;
using NLog.LayoutRenderers;

namespace Geek.Server.Gateway.Logic
{
    internal class GateStartUp
    {
        static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public static async Task Enter()
        {
            try
            {
                var flag = Start();
                if (!flag) return; //启动服务器失败

                Log.Info("进入游戏主循环...");
                Console.WriteLine("***进入游戏主循环***");
                PolymorphicRegister.Load();
                MsgHanderFactory.Init();
                HttpHanderFactory.Init();
                await HttpServer.Start(Settings.HttpPort);
                await OuterTcpServer.Start(Settings.TcpPort);
                await InnerTcpServer.Start(Settings.InsAs<GateSettings>().InnerTcpPort);
                Settings.LauchTime = DateTime.Now;
                Settings.AppRunning = true;


                //连接中心rpc
                await GateNetHelper.ConnectCenter();
                //上报注册中心
                var node = new NetNode
                {
                    NodeId = Settings.ServerId,
                    Ip = Settings.LocalIp,
                    TcpPort = Settings.TcpPort,
                    InnerTcpPort = Settings.InsAs<GateSettings>().InnerTcpPort,
                    HttpPort = Settings.HttpPort,
                    Type = NodeType.Gateway
                };
                await GateNetHelper.CenterRpcClient.ServerAgent.Register(node);

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
            await HttpServer.Stop();
            await OuterTcpServer.Stop();
            await InnerTcpServer.Stop();
            Console.WriteLine($"退出服务器成功");
        }

        private static bool Start()
        {
            try
            {
                Console.WriteLine("init NLog config...");
                LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/gate_log.config");
                LogManager.AutoShutdown = false;
                Settings.Load<GateSettings>("Configs/gate_config.json", ServerType.Gate);
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
