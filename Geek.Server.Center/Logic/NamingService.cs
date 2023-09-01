
using Core.Discovery;
using System.Collections.Concurrent;

namespace Geek.Server.Center.Logic
{
    public class NamingService
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        //服务器节点的id 为自身的serverid
        internal readonly ConcurrentDictionary<long, ServerInfo> nodeMap = new();
         
        public int NodeCount()
        {
            return nodeMap.Count;
        }

        public ServerInfo Remove(long id)
        {
            nodeMap.TryRemove(id, out var node);
            return node;
        }

        public void Remove(ServerInfo node)
        {
            nodeMap.TryRemove(node.ServerId, out var _);
        }

        public ServerInfo Get(long id)
        {
            nodeMap.TryGetValue(id, out var v);
            return v;
        }
        public List<long> GetKeys()
        {
            var list = new List<long>();
            list.AddRange(nodeMap.Keys.ToArray());
            return list;
        }

        public List<ServerInfo> GetAllNodes()
        {
            return nodeMap.Values.ToList();
        }

        public int GetNodeCount()
        {
            return nodeMap.Count;
        }

        public ServerInfo Add(ServerInfo node)
        {
            Log.Debug($"新的网络节点:{node.ServerId} {node.Type}");
            var old = nodeMap.GetOrAdd(node.ServerId, node);
            return old != node ? old : null;
        }

        public List<ServerInfo> GetNodesByType(ServerType type)
        {
            var list = new List<ServerInfo>();
            foreach (var node in nodeMap)
            {
                if (node.Value.Type == type)
                {
                    list.Add(node.Value);
                }
            }
            return list;
        }

        public void SetNodeState(long nodeId, ServerState state)
        {
            //Log.Debug($"设置节点{nodeId}状态");
            if (nodeMap.TryGetValue(nodeId, out var node))
            {
                node.State = state;
                ServiceManager.SubscribeService.Publish(SubscribeEvent.NetNodeStateChange(node.Type), MessagePack.MessagePackSerializer.Serialize(node));
            }
        }
    }
}
