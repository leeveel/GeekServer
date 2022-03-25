using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server
{
    /// <summary>
    /// Global Event Dispatcher
    /// </summary>
    public class GlobalEventDispatcher
    {
        class EventInfo
        {
            public long EntityId { get; set; }
            public string AgentHandler { get; set; }
        }


        readonly static WorkerActor lineActor = new WorkerActor();
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        readonly static Dictionary<int, List<EventInfo>> eventHandlers = new Dictionary<int, List<EventInfo>>();

        public static void AddListener(long entityId, int evtType, string agentHandlerType)
        {
            lineActor.SendAsync(() =>
            {
                if (eventHandlers.ContainsKey(evtType))
                {
                    var list = eventHandlers[evtType];
                    for (int i = list.Count - 1; i >= 0; --i)
                    {
                        if (list[i].EntityId == entityId && list[i].AgentHandler == agentHandlerType)
                        {
                            list.RemoveAt(i);
                            break;
                        }
                    }
                }
                else
                {
                    eventHandlers.Add(evtType, new List<EventInfo>());
                }
                eventHandlers[evtType].Add(new EventInfo()
                {
                    EntityId = entityId,
                    AgentHandler = agentHandlerType,
                });
            });
        }

        public static void RemoveListener(long entityId, int evtType, string agentHandlerType)
        {
            lineActor.SendAsync(() =>
            {
                if (!eventHandlers.ContainsKey(evtType))
                    return;
                var list = eventHandlers[evtType];
                for (int i = list.Count - 1; i >= 0; --i)
                {
                    if (list[i].AgentHandler == agentHandlerType && list[i].EntityId == entityId)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            });
        }

        public static void ClearListener(long entityId)
        {
            lineActor.SendAsync(() =>
            {
                var evtList = new List<int>(eventHandlers.Keys);
                foreach (var evt in evtList)
                {
                    var list = eventHandlers[evt];
                    for (int i = list.Count - 1; i >= 0; --i)
                    {
                        if (list[i].EntityId == entityId)
                        {
                            list.RemoveAt(i);
                            break;
                        }
                    }
                }
            });
        }

        public static void DispatchEvent(int evtType, Func<int, long, Type, Task<bool>> checkDispatchFunc, Param param = null)
        {
            lineActor.SendAsync(async () =>
            {
                if (!eventHandlers.ContainsKey(evtType))
                    return;

                Event evt = new Event();
                evt.EventId = evtType;
                evt.Data = param;
                var list = eventHandlers[evtType];
                foreach (var evtInfo in list)
                {
                    var info = evtInfo; //需要存个临时变量
                    var handler = HotfixMgr.GetInstance<IEventListener>(info.AgentHandler);
                    var agentType = handler.GetType().BaseType.GenericTypeArguments[0];
                    var comp = await EntityMgr.GetCompAgent(evtInfo.EntityId, agentType);
                    if (comp == null)
                        continue;

                    _ = comp.Owner.Actor.SendAsync(async ()=>
                    {
                        try
                        {
                            //component
                            var compType = agentType.BaseType.GenericTypeArguments[0];
                            if (checkDispatchFunc != null && !(await checkDispatchFunc(evtType, info.EntityId, compType)))
                                return;
                            await handler.InnerHandleEvent(comp, evt);
                        }
                        catch (Exception e)
                        {
                            LOGGER.Error(e.ToString());
                        }
                    });
                }
            });
        }
    }
}
