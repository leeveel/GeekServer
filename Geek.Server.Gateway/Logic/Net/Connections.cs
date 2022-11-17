using System.Collections.Concurrent;

namespace Geek.Server.Gateway.Logic.Net
{

    public class Connection
    {
        public long Id { get; set; }
        public long TargetId { get; set; }

        public NetChannel Channel { get; set; }
    }

    internal class Connections
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        //服务器节点的id 为自身的serverid
        internal readonly ConcurrentDictionary<long, Connection> connMap = new();

        public Connection Remove(long id)
        {
            connMap.TryRemove(id, out var node);
            return node;
        }

        public void Remove(Connection node)
        {
            connMap.TryRemove(node.Id, out var _);
        }

        public Connection Get(long id)
        {
            connMap.TryGetValue(id, out var v);
            return v;
        }
        public List<long> GetKeys()
        {
            var list = new List<long>();
            list.AddRange(connMap.Keys.ToArray());
            return list;
        }

        public List<Connection> GetAllNodes()
        {
            return connMap.Values.ToList();
        }

        public int GetNodeCount()
        {
            return connMap.Count;
        }

        public Connection Add(Connection node)
        {
            Log.Debug($"新的网络节点:{node.Id}");
            var old = connMap.GetOrAdd(node.Id, node);
            return old != node ? old : null;
        }

    }
}
