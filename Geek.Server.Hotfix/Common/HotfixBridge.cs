
using Geek.Server.App.Common.Net;
using Geek.Server.App.Common.Session;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Net.Http;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Net.Websocket;
using Geek.Server.Core.Timer;
using Geek.Server.Core.Utils;
using Microsoft.AspNetCore.Connections;
using PolymorphicMessagePack;

namespace Server.Logic.Common
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
            PolymorphicTypeMapper.Register(this.GetType().Assembly);
            HotfixMgr.SetMsgGetter(MsgFactory.GetType);

            await TcpServer.Start(Settings.TcpPort, builder => builder.UseConnectionHandler<AppTcpConnectionHandler>());
            await WebSocketServer.Start(Settings.WebSocketUrl, new AppWebSocketConnectionHandler());
            await HttpServer.Start(Settings.HttpPort);

            Log.Info("load config data");
            (bool success, string msg) = GameDataManager.ReloadAll();
            if (!success)
                throw new Exception($"载入配置表失败... {msg}");

            GlobalTimer.Start();
            await CompRegister.ActiveGlobalComps();
            return true;
        }

        public async Task Stop()
        {
            try
            {
                // 断开所有连接
                await SessionManager.RemoveAll();
                // 取消所有未执行定时器
                await QuartzTimer.Stop();
                // 保证actor之前的任务都执行完毕
                await ActorMgr.AllFinish();
                // 关闭网络服务
                await HttpServer.Stop();
                await TcpServer.Stop();
                await WebSocketServer.Stop();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                // 存储所有数据
                await GlobalTimer.Stop();
                await ActorMgr.RemoveAll();
            }
        }
    }
}
