using Core.Discovery;
using Geek.Server.Core.Net;
using Grpc.Net.Client;
using MagicOnion.Client;

namespace Geek.Server.Core.Discovery
{
    public abstract class BaseDiscoveryClient : IDiscoveryClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public IDiscoveryHub ServerAgent { private set; get; }
        protected string connUrl;
        protected ReConnecter reConn;
        protected Func<ServerState> selfNodeStateGetter;
        protected Func<ServerInfo> selfNodeGetter;
        CancellationTokenSource cancelStateSyncSrc;

        public BaseDiscoveryClient(string url, Func<ServerInfo> selfNodeGetter, Func<ServerState> selfNodeStateGetter = null)
        {
            connUrl = url;
            this.selfNodeGetter = selfNodeGetter;
            this.selfNodeStateGetter = selfNodeStateGetter;
            reConn = new ReConnecter(ConnectImpl, $"中心服:{connUrl}");
        }

        async Task<bool> Register()
        {
            var ret = await ServerAgent.Register(selfNodeGetter());
            _ = StartSyncStateAsync();
            //ServerChanged(await ServerAgent.GetAllNodes());
            return ret;
        }

        async Task StartSyncStateAsync()
        {
            if (selfNodeStateGetter == null)
                return;

            if (cancelStateSyncSrc != null)
            {
                cancelStateSyncSrc.Cancel();
                cancelStateSyncSrc = null;
            }

            cancelStateSyncSrc = new CancellationTokenSource();

            while (!cancelStateSyncSrc.IsCancellationRequested)
            {
                try
                {
                    if (ServerAgent != null)
                        await ServerAgent.SyncState(selfNodeStateGetter());
                }
                catch (Exception ex)
                {
                    //LOGGER.Error($"rpc.同步状态到中心服异常:{ex.Message}");
                }
                await Task.Delay(1000_0);
            };
        }

        public Task Start()
        {
            return reConn.Connect();
        }

        public async Task Stop()
        {
            if (cancelStateSyncSrc != null)
            {
                cancelStateSyncSrc.Cancel();
                cancelStateSyncSrc = null;
            }
            if (ServerAgent != null)
                await ServerAgent.DisposeAsync();
        }

        private async Task<bool> ConnectImpl()
        {
            try
            {
                var channel = GrpcChannel.ForAddress(connUrl);
                ServerAgent = await StreamingHubClient.ConnectAsync<IDiscoveryHub, IDiscoveryClient>(channel, this);
                if (selfNodeGetter != null)
                {
                    var ret = await Register();
                    if (!ret)
                    {
                        LOGGER.Error("连接center服，注册信息失败...");
                    }
                }
                _ = RegisterDisconnectEvent();
                return true;
            }
            catch (Exception e)
            {
                LOGGER.Error($"rpc连接异常:{e.Message}");
                try
                {
                    if (ServerAgent != null)
                        await ServerAgent.DisposeAsync();
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"rpc.Dispose异常:{ex.Message}");
                }
                ServerAgent = null;
                return false;
            }
        }

        private async Task RegisterDisconnectEvent()
        {
            try
            {
                await ServerAgent.WaitForDisconnect();
            }
            catch (Exception e)
            {
                LOGGER.Error($"RegisterDisconnectEvent.rpc异常:{e.Message}");
            }
            finally
            {
                LOGGER.Info("disconnected from the discovery server.");
                await reConn.ReConnect();
            }
        }

        public abstract void ServerChanged(List<ServerInfo> nodes);

        public abstract void HaveMessage(string eid, byte[] msg);
    }
}
