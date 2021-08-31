using System;
using Geek.Server.Config;
using System.Threading.Tasks;
using System.Collections.Generic;
using Geek.Server.Logic.Server;

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
                if (isReload)
                {
                    //热更
                    LOGGER.Info("load配置表...");
                    //配置表代码在hotfix，热更后当前域的GameDataManager=null，需要重新加载配置表
                    (bool beanSuccess, string msg) = GameDataManager.ReloadAll();
                    if(!beanSuccess)
                    {
                        LOGGER.Info("load配置表异常...");
                        LOGGER.Error(msg);
                        return false;
                    }

                    LOGGER.Info("清除缓存的agent...");
                    await ActorManager.ActorsForeach((actor) =>
                    {
                        actor.SendAsync(actor.ClearCacheAgent, true);
                        return Task.CompletedTask;
                    });
                    LOGGER.Info("hotfix load success");
                }
                else
                {
                    //起服
                    if (!await Start())
                        return false;
                }
				
				//成功了才替换msg&handler
				HttpHandlerFactory.SetHandlerGetter(HotfixMgr.GetHttpHandler);
                TcpHandlerFactory.SetHandlerGetter(Message.MsgFactory.Create, msgId => HotfixMgr.GetHandler<BaseTcpHandler>(msgId));
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

                LOGGER.Info("load配置表...");
                (bool beanSuccess, string msg) = GameDataManager.ReloadAll();
                if (!beanSuccess)
                {
                    LOGGER.Error(msg);
                    return false;
                }

                LOGGER.Info("自动激活全局actor...");
                var taskList = new List<Task>();
                var list = ComponentMgr.Singleton.AutoActiveActorList;
                for (int i = 0; i < list.Count; i++)
                {
                    var task = ActorMgr.GetOrNew((ActorType)list[i]);
                    taskList.Add(task);
                }
                await Task.WhenAll(taskList);

                LOGGER.Info("起服检查是否跨天...");
                var serverComp = await ActorMgr.GetCompAgent<ServerCompAgent>(ActorType.Server);
                _ = serverComp.CheckCrossDay();

                return true;
            }
            catch(Exception e)
            {
                LOGGER.Fatal("起服失败\n" + e.ToString());
                return false;
            }
        }


        public async Task<bool> Stop()
        {
            try
            {
                await QuartzTimer.Stop();
                await SessionManager.RemoveAll();
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
