using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using Core.Discovery;
using Geek.Server.Core.Net.Kcp;
using Geek.Server.Core.Utils;
using MagicOnion.Server.Hubs;
using NLog;

namespace Geek.Server.Discovery.Logic
{
    public class DiscoveryHub : StreamingHubBase<IDiscoveryHub, IDiscoveryClient>, IDiscoveryHub
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        const string globalGroupName = "global";

        private IGroup group; 

        static ConcurrentDictionary<int, DiscoveryHub> gatewayHubDic = new(); 

        CancellationTokenSource closeSrc = new();

        ServerInfo info;
        public long CurServerId => (long)(info?.ServerId);

        protected override async ValueTask OnConnecting()
        {
            LOGGER.Info($"rpc客户端连接: {Context.CallContext.Peer}");
            group = await Group.AddAsync(globalGroupName);
        }

        protected override async ValueTask OnDisconnected()
        {
            closeSrc.Cancel();

            if (group != null)
                await group.RemoveAsync(Context);

            if (info == null)
                return;

            LOGGER.Debug($"rpc客户端断开连接:[{info}] {Context.CallContext.Peer}");  

            NamingService.Instance.Remove(info); 
            await NodesChanged();
        }

        private async Task NodesChanged()
        {
            await Task.Delay(10);
            var list = NamingService.Instance.GetAllNodes();
            Broadcast(group).ServerChanged(list);
        }

        /// <summary>
        /// 所有服启动之后都要到中心服注册
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Register(ServerInfo info)
        {
            this.info = info;
            NamingService.Instance.Add(info);
            await NodesChanged();
            //如果是网关节点，开始心跳检测用于判断是否压力过大
            if (info.Type == ServerType.Gate)
            {
                gatewayHubDic[info.ServerId] = this; 
                _ = ExceptionMonitor.Report(ExceptionType.Notify, $"网关服[{info}]上线...");
            }
            return true;
        }

        public Task<List<ServerInfo>> GetAllNodes()
        {
            var nodes = NamingService.Instance.GetAllNodes();
            return Task.FromResult(nodes);
        }

        public Task<List<ServerInfo>> GetNodesByType(ServerType type)
        {
            var nodes = NamingService.Instance.GetNodesByType(type);
            return Task.FromResult(nodes);
        }

        //节点主动同步状态
        public Task SyncState(ServerState state)
        {
            NamingService.Instance.SetNodeState(CurServerId, state);
            return Task.CompletedTask;
        }
    }
}
