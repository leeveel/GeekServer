using Geek.Server.Center.Web.Data;
using Geek.Server.Center.Web.Pages.Config;
using Geek.Server.Core.Center;
using MagicOnion.Server.Hubs;
using NLog;
using System.Collections.Concurrent;
using System.Security.Policy;

namespace Geek.Server.Center.Logic
{
    /// <summary>
    /// 一个客户端连接对应一个hub实例
    /// </summary>
    public class CenterRpcHub : StreamingHubBase<ICenterRpcHub, ICenterRpcClient>, ICenterRpcHub
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        const string globalGroupName = "global";

        private IGroup group;

        private ConcurrentDictionary<string, bool> SubscribeEvts = new();

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
            UnsubscribeAll();
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


        public Task<ConfigInfo> GetConfig(string configId)
        {
            var cfg = ServiceManager.ConfigService.GetConfig(configId);
            Subscribe(SubscribeEvent.ConfigChange);
            return Task.FromResult(cfg);
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

        public Task Subscribe(string eventId)
        {
            SubscribeEvts[eventId] = true;
            ServiceManager.SubscribeService.Subscribe(eventId, this);
            return Task.CompletedTask;
        }
        public Task Unsubscribe(string eventId)
        {
            SubscribeEvts.TryRemove(eventId, out _);
            ServiceManager.SubscribeService.Unsubscribe(eventId, this);
            return Task.CompletedTask;
        }

        public Task UnsubscribeAll()
        {
            foreach (var v in SubscribeEvts)
            {
                ServiceManager.SubscribeService.Unsubscribe(v.Key, this);
            }
            SubscribeEvts.Clear();
            return Task.CompletedTask;
        }

        public void PostMessageToClient(string eid, string msg)
        {
            GetRpcClientAgent().HaveMessage(eid, msg);
        }
    }
}
