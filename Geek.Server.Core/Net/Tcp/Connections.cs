using Common.Net.Tcp;
using System.Collections;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Geek.Server.Core.Net.Tcp
{
    public class Connections
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        //服务器节点的id 为自身的serverid
        internal readonly ConcurrentDictionary<long, INetChannel> connMap = new();

        public INetChannel Remove(long id)
        {
            connMap.TryRemove(id, out var node);
            return node;
        }

        public void Remove(INetChannel channel)
        {
            connMap.TryRemove(channel.NetId, out var _);
        }

        public INetChannel Get(long id)
        {
            connMap.TryGetValue(id, out var c);
            //if (c == null)
            //    Log.Debug($"不能发现节点:{id},可能已经断开连接");
            return c;
        }

        public int GetConnectionCount()
        {
            return connMap.Count;
        }

        public INetChannel Add(INetChannel node)
        {
            if (node.IsClose())
                return node;
            Log.Debug($"新的网络节点:{node.NetId}");
            connMap.TryRemove(node.NetId, out var old);
            connMap.TryAdd(node.NetId, node);
            return old != node ? old : null;
        }

        public List<long> GetAllNodeWithTargetId(long target)
        {
            var list = new List<long>();
            foreach (var kv in connMap)
            {
                if (kv.Key == target)
                {
                    list.Add(kv.Key);
                }
            }
            return list;
        }

        public List<long> GetAllNodeIds(List<long> list = null)
        {
            if (list == null)
                list = new List<long>();
            foreach (var kv in connMap)
            {
                list.Add(kv.Key);
            }
            return list;
        }
    }
}
