using Geek.Server.Core.Center;
using Geek.Server.Core.Net.Http;
using PolymorphicMessagePack;
using Geek.Server.Gateway.Net;
using Geek.Server.Gateway.Net.Http;
using Geek.Server.Gateway.Net.Tcp.Handler;
using Geek.Server.Proto;
using MongoDB.Driver.Core.Servers;
using Newtonsoft.Json;
using NLog.LayoutRenderers;
using Protocol;

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

                PolymorphicRegister.Load();
                PolymorphicResolver.Init();

                MsgHanderFactory.Init();
                HttpHanderFactory.Init();
                await HttpServer.Start(Settings.HttpPort);
                await GateNetMgr.StartTcpServer();
                Settings.LauchTime = DateTime.Now;
                Settings.AppRunning = true;

                _ = Task.Run(async () =>
                {
                    //连接中心rpc
                    if (await GateNetMgr.ConnectCenter())
                    {
                        var nodeGetter = () => new NetNode
                        {
                            NodeId = Settings.ServerId,
                            Ip = Settings.LocalIp,
                            TcpPort = Settings.TcpPort,
                            InnerIp = Settings.InsAs<GateSettings>().InnerIp,
                            InnerTcpPort = Settings.InsAs<GateSettings>().InnerTcpPort,
                            HttpPort = Settings.HttpPort,
                            Type = NodeType.Gateway,
                        };

                        var stateGetter = () =>
                        {
                            var state = new NetNodeState();
                            state.MaxLoad = Settings.InsAs<GateSettings>().MaxClientCount;
                            state.CurrentLoad = GateNetMgr.GetClientConnectionCount();
                            return state;
                        };

                        //上报注册中心
                        if (!await GateNetMgr.CenterRpcClient.Register(nodeGetter, stateGetter))
                            throw new Exception($"中心服注册失败... {JsonConvert.SerializeObject(nodeGetter())}");
                    }
                });
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
            await GateNetMgr.Stop();
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
