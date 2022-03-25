using Geek.Server.Config;
using Geek.Server.Logic.Role;
using Geek.Server.Logic.Server;
using Geek.Server.Message.Login;
using System;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class HotfixBridge : IHotfixBridge
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public ServerType BridgeType => ServerType.Game;

        async Task Start()
        {
            LOGGER.Info("\n\n\nstart server...");
            MsgFactory.InitMsg(typeof(ReqLogin));
            HttpHandlerFactory.SetHandlerGetter(HotfixMgr.GetHttpHandler);
            TcpHandlerFactory.SetHandlerGetter(MsgFactory.GetMsg, msgId => HotfixMgr.GetHandler<BaseTcpHandler>(msgId));

            await TcpServer.Start(Settings.Ins.TcpPort, Settings.Ins.UseLibuv);
            await HttpServer.Start(Settings.Ins.httpPort);
            //RedisMgr.Init();
            //ServerInfoUtils.Init();
            //GrpcServer.Init(Settings.Ins.GrpcPort);
            //ConsulUtils.Init(Settings.Ins.configCenterUrl);

            LOGGER.Info($"connect mongo {Settings.Ins.mongoDB} {Settings.Ins.mongoUrl}...");
            MongoDBConnection.Singleton.Connect(Settings.Ins.mongoDB, Settings.Ins.mongoUrl);


            GlobalDBTimer.Singleton.Start();

            LOGGER.Info("load bean...");
            var ret = GameDataManager.ReloadAll();
            if (!ret.Item1)
            {
                LOGGER.Error("加载配置表异常，起服失败");
                throw new Exception(ret.Item2);
            }

            LOGGER.Info("index mongodb...");
            await MongoDBConnection.Singleton.IndexCollectoinMore<RoleState>(MongoField.Name);

            EntityMgr.Type2ID = EntityID.GetEntityIdFromType;
            EntityMgr.ID2Type = EntityID.GetEntityTypeFromID;
        }

        public async Task<bool> OnLoadSucceed(bool isReload)
        {
            if (isReload)
            {
                LOGGER.Info("hotfix load success");
                EntityMgr.ClearEntityAgent();
                HttpHandlerFactory.SetHandlerGetter(HotfixMgr.GetHttpHandler);
                TcpHandlerFactory.SetHandlerGetter(MsgFactory.GetMsg, msgId => HotfixMgr.GetHandler<BaseTcpHandler>(msgId));
                return true;
            }

            await Start();
            var serverId = Settings.Ins.ServerId;
            await EntityMgr.CompleteActiveTask();
            var serverComp = await EntityMgr.GetCompAgent<ServerCompAgent>(EntityType.Server);
            await serverComp.CheckCrossDay();
            return true;
        }

        public Task Reload()
        {
            LOGGER.Info("reload hotfix");
            return Task.CompletedTask;
        }

        public async Task<bool> Stop()
        {
            LOGGER.Info("stop hotfix");
            await SessionManager.RemoveAll();
            await QuartzTimer.Stop();
            await GlobalDBTimer.Singleton.OnShutdown();
            //await EntityMgr.RemoveAll();
            await HttpServer.Stop();
            await TcpServer.Stop();
            //ServerInfoUtils.Stop();
            //await GrpcServer.Stop().WaitAsync(TimeSpan.FromSeconds(10));
            //await GrpcClient.Showdown().WaitAsync(TimeSpan.FromSeconds(10));
            return true;
        }

    }
}
