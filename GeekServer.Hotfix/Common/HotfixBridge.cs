
namespace Geek.Server
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

            //TcpHandlerFactory.SetHandlerGetter(Geek.Server.Proto.MsgFactory.GetType, msgId => HotfixMgr.GetTcpHandler(msgId));
            //HttpHandlerFactory.SetHandlerGetter(HotfixMgr.GetHttpHandler);
            HotfixMgr.SetMsgGetter(MsgFactory.GetType);

            await TcpServer.Start(Settings.TcpPort);
            Log.Info("tcp 服务启动完成...");
            await HttpServer.Start(Settings.HttpPort);
            MongoDBConnection.Init(Settings.MongoUrl, Settings.DbName);
            Log.Info("check restore data from file");
            FileBackupStatus fileBackupFlag = FileBackup.CheckRestoreFromFile();
            switch (fileBackupFlag)
            {
                case FileBackupStatus.StoreToDbFailed:
                    throw new Exception($"上次停服回存数据失败，本次存入数据库失败");
                case FileBackupStatus.StoreToDbSuccess:
                    Log.Warn($"上次停服回存数据失败，本次存入数据库成功");
                    break;
                case FileBackupStatus.NoFile:
                default:
                    Log.Info($"上次停服回存数据没有异常，不用从磁盘中恢复数据");
                    break;
            }
            (bool success, string msg) = GameDataManager.ReloadAll();
            if (!success)
                throw new Exception($"载入配置表失败... {msg}");

            GlobalTimer.Start();

            await CompRegister.ActiveGlobalComps();
            return true;
        }

        public async Task Stop()
        {
            // 断开所有连接
            await (HotfixMgr.SessionMgr?.RemoveAll() ?? Task.CompletedTask);
            // 取消所有未执行定时器
            await QuartzTimer.Stop();
            // 保证actor之前的任务都执行完毕
            await ActorMgr.AllFinish();
            // 关闭网络服务
            await HttpServer.Stop();
            await TcpServer.Stop();
            // 存储所有数据
            await GlobalTimer.Stop();
            await ActorMgr.RemoveAll();
        }
    }
}
