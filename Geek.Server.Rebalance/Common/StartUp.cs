using Geek.Server.Core.Center;
using Geek.Server.Core.Net.Http;
using PolymorphicMessagePack;
using Geek.Server.Proto;
using Geek.Server.Rebalance.Net.Http;
using Geek.Server.Rebalance.Net.Rpc;
using Newtonsoft.Json;
using NLog.LayoutRenderers;

namespace Geek.Server.Rebalance.Common
{
    internal class StartUp
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        static CenterRpcClient CenterRpcClient;
        public static async Task Enter()
        {
            try
            {
                var flag = Start();
                if (!flag) return; //启动服务器失败

                PolymorphicRegister.Load();
                PolymorphicResolver.Init();

                Log.Info("进入主循环...");
                Console.WriteLine("***进入主循环***");
                HttpHanderFactory.Init();
                await HttpServer.Start(Settings.HttpPort);
                Settings.LauchTime = DateTime.Now;
                Settings.AppRunning = true;

                _ = Task.Run(async () =>
                {
                    CenterRpcClient = new CenterRpcClient(Settings.CenterUrl);
                    //连接中心rpc
                    if (await CenterRpcClient.Connect())
                    {
                        var node = new NetNode
                        {
                            NodeId = Settings.ServerId,
                            Ip = Settings.LocalIp,
                            HttpPort = Settings.HttpPort,
                            Type = NodeType.GatewaySelect,
                        };
                        //上报注册中心
                        if (!await CenterRpcClient.ServerAgent.Register(node))
                            throw new Exception($"中心服注册失败... {JsonConvert.SerializeObject(node)}");

                        //得到所有gateway信息
                        GatewayMgr.ResetAllNode(await CenterRpcClient.ServerAgent.GetNodesByType(NodeType.Gateway));
                        await CenterRpcClient.ServerAgent.Subscribe(SubscribeEvent.NetNodeStateChange(NodeType.Gateway));
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
            await CenterRpcClient.Stop();
            Console.WriteLine($"退出服务器成功");
        }

        private static bool Start()
        {
            try
            {
                Settings.Load<GateSelectSettings>("Configs/rebalance_config.json", ServerType.Gate);
                Console.WriteLine("init NLog config...");
                LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/rebalance_log.config");
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
