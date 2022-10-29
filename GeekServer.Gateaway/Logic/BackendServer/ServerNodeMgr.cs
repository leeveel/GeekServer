using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.BackendServer
{
    public class ServerNodeMgr
    {
        internal static readonly ConcurrentDictionary<long, StreamServer> serverMap = new();

        public static void Remove(long id)
        {
            serverMap.TryRemove(id, out var _);
        }

        public static void RemoveAll()
        {
            serverMap.Clear();
        }

        public static StreamServer Get(long id)
        {
            serverMap.TryGetValue(id, out var v);
            return v;
        }

        public static StreamServer Add(long id, StreamServer server)
        {
            var old = serverMap.GetOrAdd(id, server);
            return old != server ? old : null;
        }
    }
}
