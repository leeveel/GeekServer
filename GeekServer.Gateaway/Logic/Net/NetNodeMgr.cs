
using NLog.Fluent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.Net
{
    public class NetNodeMgr
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        //服务器节点的id 为自身的serverid
        internal static readonly ConcurrentDictionary<long, INetNode> nodeMap = new();

        public static INetNode Remove(long id)
        {
            nodeMap.TryRemove(id, out var node);
            return node;
        }

        public static void Remove(INetNode node)
        {
            nodeMap.TryRemove(node.uid, out var _);
        }

        public static INetNode Get(long id)
        {
            nodeMap.TryGetValue(id, out var v);
            return v;
        }

        public static INetNode Add(INetNode node)
        {
            Log.Debug($"新的网络节点:{node.uid} {node.type}");
            var old = nodeMap.GetOrAdd(node.uid, node);
            return old != node ? old : null;
        }
    }
}
