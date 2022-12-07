using Geek.Server.Core.Net;
using Grpc.Net.Client;
using MagicOnion.Client;
using System.Threading;

namespace Geek.Server.Core.Center
{
    public abstract class BaseCenterRpcClient : ICenterRpcClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public ICenterRpcHub ServerAgent { private set; get; }
        protected string connUrl;
        protected ReConnecter reConn;
        protected Func<NetNodeState> selfNodeStateGetter;
        protected Func<NetNode> selfNodeGetter;
        CancellationTokenSource cancelStateSyncSrc;

        public BaseCenterRpcClient(string ip, int port)
        {
            connUrl = $"http://{ip}:{port}";
            reConn = new ReConnecter(ConnectImpl, $"中心服:{connUrl}");
        }

        public BaseCenterRpcClient(string url)
        {
            connUrl = url;
            reConn = new ReConnecter(ConnectImpl, $"中心服:{connUrl}");
        }

        public async Task<bool> Register(Func<NetNode> selfNodeGetter, Func<NetNodeState> selfNodeStateGetter = null)
        {
            this.selfNodeGetter = selfNodeGetter;
            this.selfNodeStateGetter = selfNodeStateGetter;
            var ret = await ServerAgent.Register(selfNodeGetter());
            StartSyncState();
            return ret;
        }

        void StartSyncState()
        {
            if (selfNodeStateGetter == null)
                return;
            if (Settings.SyncStateToCenterInterval <= 0.0001)
            {
                LOGGER.Error($"开始向中心服同步状态失败，SyncStateToCenterInterval参数为{Settings.SyncStateToCenterInterval}");
                return;
            }

            if (cancelStateSyncSrc != null)
            {
                cancelStateSyncSrc.Cancel();
                cancelStateSyncSrc = null;
            }

            cancelStateSyncSrc = new CancellationTokenSource();
            var token = cancelStateSyncSrc.Token;
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (ServerAgent != null)
                            await ServerAgent.SyncState(selfNodeStateGetter());
                    }
                    catch (Exception ex)
                    {
                        LOGGER.Error($"rpc.同步状态到中心服异常:{ex.Message}");
                    }
                    await Task.Delay((int)(Settings.SyncStateToCenterInterval * 1000));
                }
            });
        }

        public Task<bool> Connect()
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
                ServerAgent = await StreamingHubClient.ConnectAsync<ICenterRpcHub, ICenterRpcClient>(channel, this);
                if (selfNodeGetter != null)
                {
                    var ret = await ServerAgent.Register(selfNodeGetter());
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
                LOGGER.Info("disconnected from the center server.");
                await reConn.ReConnect();
            }
        }

        public abstract void ConfigChanged(ConfigInfo data);

        public abstract void NodesChanged(List<NetNode> nodes);

        public abstract void HaveMessage(string eid, byte[] msg);
    }
}
