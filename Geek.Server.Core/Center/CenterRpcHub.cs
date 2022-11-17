using MagicOnion;
using MagicOnion.Server.Hubs;
using NLog;

namespace Geek.Server.Core.Center
{

    public interface ICenterRpcClient
    {
        public void Connected(long id);

        public void Disconnected(long id);

        public void ConfigChanged(byte[] data);

        public void NodesChanged(List<NetNode> nodes);
    }

    public interface ICenterRpcHub : IStreamingHub<ICenterRpcHub, ICenterRpcClient>
    {
        public Task<bool> Register(NetNode info);
        public Task<byte[]> GetConfig(string configId);
        public Task<bool> SetConfig(string configId, byte[] data);
        public Task<List<NetNode>> GetAllNodes();
        public Task<List<NetNode>> GetNodeByType(NodeType type);
    }

    /// <summary>
    /// 一个客户端连接对应一个hub实例
    /// </summary>
    public class CenterRpcHub : StreamingHubBase<ICenterRpcHub, ICenterRpcClient>, ICenterRpcHub
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        const string globalGroupName = "global";

        private IGroup group;

        public long CurNodeId { private set; get; }

        protected override async ValueTask OnConnecting()
        {
            LOGGER.Debug($"rpc客户端连接:{Context.CallContext.Peer}");
            group = await Group.AddAsync(globalGroupName);
        }

        protected override ValueTask OnDisconnected()
        {
            LOGGER.Debug($"rpc客户端断开连接:{Context.CallContext.Peer}");
            ServiceManager.NamingService.Remove(CurNodeId);
            if (group != null)
                group.RemoveAsync(Context);
            //group = null;
            NodesChanged();
            return ValueTask.CompletedTask;
        }

        public ICenterRpcClient GetRpcClientAgent()
        {
            return BroadcastToSelf(group);
        }

        private void NodesChanged()
        {
            Task.Run(() =>
            {
                var list = ServiceManager.NamingService.GetAllNodes();
                Broadcast(group).NodesChanged(list);
            });
        }

        /// <summary>
        /// 所有服启动之后都要到中心服来注册
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Task<bool> Register(NetNode node)
        {
            CurNodeId = node.NodeId;
            ServiceManager.NamingService.Add(node);
            NodesChanged();
            return Task.FromResult(true);
        }

        public Task<byte[]> GetConfig(string configId)
        {
            var bytes = ServiceManager.ConfigService.GetConfig(configId);
            return Task.FromResult(bytes);
        }

        public Task<bool> SetConfig(string configId, byte[] data)
        {
            ServiceManager.ConfigService.GetConfig(configId, data);
            return Task.FromResult(true);
        }

        public Task<List<NetNode>> GetAllNodes()
        {
            var nodes = ServiceManager.NamingService.GetAllNodes();
            return Task.FromResult(nodes);
        }

        public Task<List<NetNode>> GetNodeByType(NodeType type)
        {
            var nodes = ServiceManager.NamingService.GetNodeByType(type);
            return Task.FromResult(nodes);
        }
    }
}
