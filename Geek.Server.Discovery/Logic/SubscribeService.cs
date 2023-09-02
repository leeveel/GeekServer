using System.Collections.Concurrent;

namespace Geek.Server.Discovery.Logic
{
    public class SubscribeService
    {
        ConcurrentDictionary<string, ConcurrentDictionary<DiscoveryHub, DiscoveryHub>> subClients = new();
        public void Subscribe(string id, DiscoveryHub hub)
        {
            var dic = GetOrAddSubscribeMap(id);
            dic[hub] = hub;
        }
        public void Unsubscribe(string id, DiscoveryHub hub)
        {
            GetOrAddSubscribeMap(id).TryRemove(hub, out _);
        }

        public void Publish(string eid, byte[] msg)
        {
            var dic = GetOrAddSubscribeMap(eid);
            foreach (var v in dic)
            {
                v.Value.PostMessageToClient(eid, msg);
            }
        }

        ConcurrentDictionary<DiscoveryHub, DiscoveryHub> GetOrAddSubscribeMap(string id)
        {
            if (subClients.TryGetValue(id, out var dic))
            {
                return dic;
            }
            dic = new ConcurrentDictionary<DiscoveryHub, DiscoveryHub>();
            subClients.TryAdd(id, dic);
            return dic;
        }
    }
}
