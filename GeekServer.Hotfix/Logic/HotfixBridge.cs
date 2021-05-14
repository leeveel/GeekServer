using System;
using Geek.Server.ConfigBean;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Geek.Server
{
    public class HotfixBridge : IHotfixBridge
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public ServerType BridgeType => ServerType.Game;
        public async Task<bool> OnLoadSucceed(bool isReload)
        {
            try
            {
                HttpHandlerFactory.SetExtraHandlerGetter(HotfixMgr.GetHttpHandler);
                TcpHandlerFactory.SetExtraHandlerGetter(Geek.Server.Message.MsgFactory.Create, msgId => HotfixMgr.GetHandler<BaseTcpHandler>(msgId));

                if (isReload)
                {
                    //热更
                    LOGGER.Info("hotfix load success");
                    await ActorManager.ActorsForeach((actor) =>
                    {
                        actor.SendAsync(actor.ClearCacheAgent, false);
                        return Task.CompletedTask;
                    });
                }else
                {
                    //起服
                    if (!await Start())
                        return false;
                }
                return true;
            }
            catch(Exception e)
            {
                LOGGER.Fatal("OnLoadSucceed执行异常");
                LOGGER.Fatal(e.ToString());
                return false;
            }
        }

        async Task<bool> Start()
        {
            try
            {
                LOGGER.Info("start server......");
                await HttpServer.Start(Settings.Ins.HttpPort);
                await TcpServer.Start(Settings.Ins.TcpPort, Settings.Ins.UseLibuv);

                LOGGER.Info("init mongodb......" + Settings.Ins.MongoUrl);
                MongoDBConnection.Singleton.Connect(Settings.Ins.MongoDB, Settings.Ins.MongoUrl);

                LOGGER.Info("启动回存timer......");
                GlobalDBTimer.Singleton.Start();

                LOGGER.Info("注册所有组件......");
                ComponentTools.RegistAllComps();

                LOGGER.Info("load配置表...");
                (bool success, string msg) = GameDataManager.ReloadAll();
                if (!success)
                    return false;

                LOGGER.Info("激活所有全局actor...");
                var taskList = new List<Task>();
                taskList.Add(activeActorAndItsComps<ServerActorAgent>(ServerActorID.GetID(ActorType.Normal)));
                //激活其他全局actor

                await Task.WhenAll(taskList);

                var serverActor = await ActorManager.GetOrNew<ServerActorAgent>(ServerActorID.GetID(ActorType.Normal));
                _ = serverActor.SendAsync(serverActor.CheckCrossDay);

                return true;
            }catch(Exception e)
            {
                LOGGER.Fatal("起服失败\n" + e.ToString());
                return false;
            }
        }

        async Task activeActorAndItsComps<TActorAgent>(long actorId) where TActorAgent : IComponentActorAgent
        {
            var actor = await ActorManager.GetOrNew<TActorAgent>(actorId);
            await ((ComponentActor)actor.Owner).ActiveAllComps();
        }

        public async Task<bool> Stop()
        {
            try
            {
                await QuartzTimer.Stop();
                await ChannelManager.RemoveAll();
                await GlobalDBTimer.Singleton.OnShutdown();
                await ActorManager.RemoveAll();
                await TcpServer.Stop();
                await HttpServer.Stop();
                return true;
            }catch(Exception e)
            {
                LOGGER.Fatal(e.ToString());
                return false;
            }
        }
    }
}
