
using Core.Discovery;
using System.Collections.Concurrent;

namespace Geek.Server.Discovery.Logic
{
    public class NamingService : Singleton<NamingService>
    {
        static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        //服务器节点的id 为自身的serverid
        ConcurrentDictionary<long, ServerInfo> serverMap = new();

        public int NodeCount()
        {
            return serverMap.Count;
        }

        public void Remove(ServerInfo info)
        {
            if (info == null)
                return;
            serverMap.TryRemove(new KeyValuePair<long, ServerInfo>(info.ServerId, info));
        }

        public ServerInfo Get(long id)
        {
            serverMap.TryGetValue(id, out var v);
            return v;
        }

        public List<ServerInfo> GetAllNodes()
        {
            return serverMap.Values.ToList();
        }

        public int GetNodeCount()
        {
            return serverMap.Count;
        }

        public void AddSelf()
        {
            serverMap[Settings.Ins.ServerId] = new ServerInfo
            {
                Type = ServerType.Discovery,
                ServerName = Settings.Ins.ServerName, 
                ServerId = Settings.Ins.ServerId,
                LocalIp = Settings.Ins.LocalIp,
                InnerPort = Settings.Ins.RpcPort
            };
        }

        public void Add(ServerInfo node)
        {
            if (node.Type == ServerType.Discovery)
            {
                Log.Error($"不能添加discovery节点...{node?.ToString()}");
                return;
            }
            serverMap[node.ServerId] = node;
            Log.Info($"新的网络节点:[{node}]   总数：{GetNodeCount()}");
        }

        public List<ServerInfo> GetNodesByType(ServerType type)
        {
            var list = new List<ServerInfo>();
            foreach (var node in serverMap)
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
            if (serverMap.TryGetValue(nodeId, out var node))
            {
                node.State = state;
            }
        }
    }
}
