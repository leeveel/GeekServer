using Geek.Server.Core.Net.Messages;
using System.Collections.Concurrent;

namespace Geek.Server.Core.Net.Tcp
{

    public class Connection
    {
        public long Id { get; set; }
        public long NodeId { get; set; }
        public NetChannel Channel { get; set; }

        public void WriteAsync(NetMessage msg)
        {
            Channel?.WriteAsync(msg);
        }

        public void WriteAsync(Message msg)
        {
            Channel?.WriteAsync(msg);
        }
    }

    public class Connections
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        //服务器节点的id 为自身的serverid
        internal readonly ConcurrentDictionary<long, Connection> connMap = new();
        //nodeId - connection.id
        internal readonly ConcurrentDictionary<long, long> nodeIdMap = new();
        //nodeId - list<connection.id>(TODO:分布式中一个大服的网络节点会存在多个)
        //internal readonly ConcurrentDictionary<long, long> nodeIdsMap = new();

        public Connection Remove(long id)
        {
            if (connMap.TryRemove(id, out var node))
                nodeIdMap.TryRemove(node.NodeId, out _);
            return node;
        }

        public void Remove(Connection node)
        {
            if (connMap.TryRemove(node.Id, out var _))
                nodeIdMap.TryRemove(node.NodeId, out _);
        }

        public Connection Get(long id)
        {
            connMap.TryGetValue(id, out var v);
            return v;
        }

        public List<Connection> GetAllConnections()
        {
            return connMap.Values.ToList();
        }

        public int GetConnectionCount()
        {
            return connMap.Count;
        }

        public Connection Add(Connection node)
        {
            Log.Debug($"新的网络节点:{node.Id}");
            var old = connMap.GetOrAdd(node.Id, node);
            return old != node ? old : null;
        }

        public Connection GetByNodeId(long nodeId)
        {
            if (nodeIdMap.TryGetValue(nodeId, out long id))
            {
                connMap.TryGetValue(id, out Connection conn);
                return conn;
            }
            return null;
        }

        public void SetNodeId(Connection conn, long nodeId)
        {
            if (conn == null)
                return;
            conn.NodeId = nodeId;
            nodeIdMap[nodeId] = conn.Id;
        }

    }
}
