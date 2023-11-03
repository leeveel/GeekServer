using System.Collections.Concurrent;
using System.Net;
using Core.Discovery;
using Geek.Server.Core.Net;
using Grpc.Net.Client;
using MagicOnion.Client;

namespace Geek.Server.Core.Discovery
{
    public abstract class BaseDiscoveryClient : IDiscoveryClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        protected IDiscoveryHub ServerAgent { private set; get; }
        protected ReConnecter reConn;
        protected Func<ServerState> selfNodeStateGetter;
        protected Func<ServerInfo> selfNodeGetter;
        protected CancellationTokenSource cancelStateSyncSrc;
        protected CancellationTokenSource closeTokenSrc;

        protected ConcurrentDictionary<long, ServerInfo> allServerMap = new();
        protected ConcurrentDictionary<long, ServerInfo> gatewayServerMap = new();

        public BaseDiscoveryClient(Func<ServerInfo> selfNodeGetter, Func<ServerState> selfNodeStateGetter = null)
        {
            this.selfNodeGetter = selfNodeGetter;
            this.selfNodeStateGetter = selfNodeStateGetter;
            closeTokenSrc = new CancellationTokenSource();
            reConn = new ReConnecter(ConnectImpl, $"DiscoveryUrl:{Settings.Ins.DiscoveryServerUrl}");
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

            cancelStateSyncSrc?.Cancel();
            cancelStateSyncSrc = new CancellationTokenSource();
            var token = cancelStateSyncSrc.Token;

            while (!cancelStateSyncSrc.IsCancellationRequested)
            {
                try
                {
                    if (ServerAgent != null)
                        await ServerAgent.SyncState(selfNodeStateGetter());
                }
                catch (Exception)
                {
                    //LOGGER.Error($"rpc.同步状态到中心服异常:{ex.Message}");
                }
                try
                {
                    await Task.Delay(2000_0, token);
                }
                catch
                {
                    break;
                }
            };
        }

        public Task Start()
        {
            LOGGER.Info($"开始{GetType().FullName}...");
            return reConn.Connect();
        }

        public async Task Stop()
        {
            cancelStateSyncSrc?.Cancel();
            closeTokenSrc?.Cancel();
            await ServerAgent?.DisposeAsync();
            ServerAgent = null;
        }

        private async Task<bool> ConnectImpl()
        {
            try
            {
                var channel = GrpcChannel.ForAddress(Settings.Ins.DiscoveryServerUrl);
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
                LOGGER.Error($"{GetType().Name}:{e.Message}");
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

        public virtual void ServerChanged(List<ServerInfo> nodes)
        {
            var allMap = new ConcurrentDictionary<long, ServerInfo>();
            var gatewayMap = new ConcurrentDictionary<long, ServerInfo>();
            LOGGER.Debug("ServerChanged:" + nodes.Count);
            foreach (var node in nodes)
            {
                try
                {
#if DEBUG
                    LOGGER.Debug(MessagePack.MessagePackSerializer.SerializeToJson(node));
#endif

                    if (string.IsNullOrEmpty(node.LocalIp))
                    {
                        node.InnerEndPoint = null;
                        LOGGER.Error($"server [{node.Type}] [{node.ServerId}] localIp is empty...");
                    }
                    else
                    {
                        node.InnerEndPoint = new IPEndPoint(IPAddress.Parse(node.LocalIp), node.InnerPort);
                    }

                    if (node.Type == ServerType.Gate)
                    {
                        gatewayMap[node.ServerId] = node;
                    }
                    allMap[node.ServerId] = node; 
                }
                catch (Exception e)
                {
                    LOGGER.Error(e);
                }
            }

            foreach (var kv in allServerMap)
            {
                if (!allMap.ContainsKey(kv.Key))
                    LOGGER.Warn($"server change remove server {kv.Value}");
            }

            allServerMap = allMap;
            gatewayServerMap = gatewayMap;
        }

        public ServerInfo GetServer(int serverId, ServerType type)
        {
            if (allServerMap.TryGetValue(serverId, out var server))
            {
                if (server.Type == type)
                    return server;
            }
            return null;
        }

        public ServerInfo GetServer(int serverId)
        {
            if (allServerMap.TryGetValue(serverId, out var server))
            {
                return server;
            }
            return null;
        }

        public EndPoint GetServerInnerEndPoint(int serverId)
        {
            if (allServerMap.TryGetValue(serverId, out var server))
            {
                return server.InnerEndPoint;
            }
            LOGGER.Error("GetServerInnerEndPoint error:"+serverId);
            return null;
        }
    }
}
