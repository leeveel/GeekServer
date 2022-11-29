using Geek.Server.Core.Center;
using System.Collections.Concurrent;

namespace Geek.Server.Center.Logic
{
    public class NamingService
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        //服务器节点的id 为自身的serverid
        internal readonly ConcurrentDictionary<long, NetNode> nodeMap = new();

        public int NodeCount()
        {
            return nodeMap.Count;
        }

        public NetNode Remove(long id)
        {
            nodeMap.TryRemove(id, out var node);
            return node;
        }

        public void Remove(NetNode node)
        {
            nodeMap.TryRemove(node.NodeId, out var _);
        }

        public NetNode Get(long id)
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

        public List<NetNode> GetAllNodes()
        {
            return nodeMap.Values.ToList();
        }

        public int GetNodeCount()
        {
            return nodeMap.Count;
        }

        public NetNode Add(NetNode node)
        {
            Log.Debug($"新的网络节点:{node.NodeId} {node.Type}");
            var old = nodeMap.GetOrAdd(node.NodeId, node);
            return old != node ? old : null;
        }

        public List<NetNode> GetNodeByType(NodeType type)
        {
            var list = new List<NetNode>();
            foreach (var node in nodeMap)
            {
                if (node.Value.Type == type)
                {
                    list.Add(node.Value);
                }
            }
            return list;
        }

        public void SetNodeState(long nodeId, NetNodeState state)
        {
            //Log.Debug($"设置节点{nodeId}状态");
            if (nodeMap.TryGetValue(nodeId, out var node))
            {
                node.State = state;
            }
        }
    }
}
