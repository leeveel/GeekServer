using Geek.Server.Core.Center;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Geek.Server.Center.Logic
{
    public class SubscribeService
    {
        ConcurrentDictionary<string, ConcurrentDictionary<CenterRpcHub, CenterRpcHub>> subClients = new();
        public void Subscribe(string id, CenterRpcHub hub)
        {
            var dic = GetOrAddSubscribeMap(id);
            dic[hub] = hub;
        }
        public void Unsubscribe(string id, CenterRpcHub hub)
        {
            GetOrAddSubscribeMap(id).TryRemove(hub, out _);
        }

        public void Publish(string eid, ConfigInfo msg)
        {
            var dic = GetOrAddSubscribeMap(eid);
            foreach (var v in dic)
            {
                v.Value.GetRpcClientAgent().ConfigChanged(msg);
            }
        }

        public void Publish(string eid, byte[] msg)
        {
            var dic = GetOrAddSubscribeMap(eid);
            foreach (var v in dic)
            {
                v.Value.PostMessageToClient(eid, msg);
            }
        }

        ConcurrentDictionary<CenterRpcHub, CenterRpcHub> GetOrAddSubscribeMap(string id)
        {
            if (subClients.TryGetValue(id, out var dic))
            {
                return dic;
            }
            dic = new ConcurrentDictionary<CenterRpcHub, CenterRpcHub>();
            subClients.TryAdd(id, dic);
            return dic;
        }
    }
}
