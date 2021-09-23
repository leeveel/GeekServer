using System;
using System.Collections.Generic;

/// <summary>
/// 网络事件监听（不允许有多个监听，多个后来者顶替前一个）
/// </summary>

namespace Geek.Client
{
    public class NetEventComp
    {
        Dictionary<int, Action<Event>> evtMap = new Dictionary<int, Action<Event>>();
        public void AddListener(int id, Action<Event> handler)
        {
            int evtId = id;
            if (!evtMap.ContainsKey(evtId))
            {
                evtMap.Add(evtId, handler);
            }
            else
            {
                //去重，一个网络消息只要一个监听
                UnityEngine.Debug.LogWarning("重复注册网络事件>" + evtId);
                GED.NED.removeListener(evtId, evtMap[evtId]);
                evtMap[evtId] = handler;
            }
            GED.NED.addListener(id, handler);
        }

        public void RemoveListener(int id, Action<Event> handler)
        {
            int evtId = id;
            if (!evtMap.ContainsKey(evtId))
                return;
            evtMap[evtId] -= handler;
            GED.NED.removeListener(id, handler);
        }

        public void RemoveListeners(int id)
        {
            int evtId = id;
            if (!evtMap.ContainsKey(evtId))
                return;
            GED.NED.removeListener(id, evtMap[evtId]);
            evtMap.Remove(evtId);
        }

        public void ClearListeners()
        {
            foreach (var kv in evtMap)
                GED.NED.removeListener(kv.Key, kv.Value);
            evtMap.Clear();
        }

    }
}
