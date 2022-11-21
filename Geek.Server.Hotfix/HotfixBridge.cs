using Geek.Server.App.Net;
using Geek.Server.App.Net.Session;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Center;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Net.Http;
using Geek.Server.Core.Timer;
using Newtonsoft.Json;

namespace Geek.Server.Hotfix
{
    internal class HotfixBridge : IHotfixBridge
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ServerType BridgeType => ServerType.Game;

        public async Task<bool> OnLoadSuccess(bool reload)
        {
            if (reload)
            {
                ActorMgr.ClearAgent();
                return true;
            }

            HotfixMgr.SetMsgGetter(MsgFactory.GetType);

            await HttpServer.Start(Settings.HttpPort);
            Log.Info("load config data");
            (bool success, string msg) = GameDataManager.ReloadAll();
            if (!success)
                throw new Exception($"载入配置表失败... {msg}");


            _ = Task.Run(async () => 
            {
                //连接中心rpc
                if (await AppNetMgr.ConnectCenter())
                {
                    //上报注册中心
                    var node = new NetNode
                    {
                        NodeId = Settings.ServerId,
                        Ip = Settings.LocalIp,
                        TcpPort = Settings.TcpPort,
                        HttpPort = Settings.HttpPort,
                        Type = NodeType.Game
                    };
                    if (!await AppNetMgr.CenterRpcClient.ServerAgent.Register(node))
                        throw new Exception($"中心服注册失败... {JsonConvert.SerializeObject(node)}");

                    //到中心服拉取通用配置
                    await AppNetMgr.GetGlobalConfig();
                }
            });

            GlobalTimer.Start();
            await CompRegister.ActiveGlobalComps();
            return true;
        }

        public async Task Stop()
        {
            // 断开所有连接
            await SessionManager.RemoveAll();
            // 取消所有未执行定时器
            await QuartzTimer.Stop();
            // 保证actor之前的任务都执行完毕
            await ActorMgr.AllFinish();
            // 关闭网络服务
            await HttpServer.Stop();
            //await TcpServer.Stop();
            // 存储所有数据
            await GlobalTimer.Stop();
            await ActorMgr.RemoveAll();
        }
    }
}
