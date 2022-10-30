
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
        internal static readonly ConcurrentDictionary<long, INetNode> nodeMap = new();

        public static void Remove(long id)
        {
            nodeMap.TryRemove(id, out var node);
        }


        public static INetNode Get(long id)
        {
            nodeMap.TryGetValue(id, out var v);
            return v;
        }


        public static INetNode Add(long id, INetNode node)
        {
            var old = nodeMap.GetOrAdd(id, node);
            return old != node ? old : null;
        }
    }
}
