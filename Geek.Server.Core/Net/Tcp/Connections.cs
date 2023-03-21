using System.Collections;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Geek.Server.Core.Net.Tcp
{
    public class Connections
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        //服务器节点的id 为自身的serverid
        internal readonly ConcurrentDictionary<long, NetChannel> connMap = new();

        public NetChannel Remove(long id)
        {
            connMap.TryRemove(id, out var node);
            return node;
        }

        public void Remove(NetChannel channel)
        {
            connMap.TryRemove(channel.NodeId, out var _);
        }

        public NetChannel Get(long id)
        {
            connMap.TryGetValue(id, out var c);
            if (c == null)
                Log.Warn($"不能发现节点:{id}");
            return c;
        }

        public int GetConnectionCount()
        {
            return connMap.Count;
        }

        public NetChannel Add(NetChannel node)
        {
            if (node.IsClose())
                return node;
            Log.Debug($"新的网络节点:{node.NodeId}");
            connMap.TryRemove(node.NodeId, out var old);
            connMap.TryAdd(node.NodeId, node);
            return old != node ? old : null;
        }
    }
}
