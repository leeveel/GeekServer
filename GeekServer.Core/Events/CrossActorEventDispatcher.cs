using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class CrossActorEventDispatcher<TActor> where TActor : ComponentActor, new()
    {
        class EventInfo
        {
            public long ActorId { get; set; }
            public string AgentHandler { get; set; }
        }


        readonly WorkerActor lineActor = new WorkerActor();
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        readonly Dictionary<int, List<EventInfo>> eventHandlers = new Dictionary<int, List<EventInfo>>();

        public void AddListener(long actorId, int evtType, string agentHandlerType)
        {
            lineActor.SendAsync(() =>
            {
                if (eventHandlers.ContainsKey(evtType))
                {
                    var list = eventHandlers[evtType];
                    for (int i = list.Count - 1; i >= 0; --i)
                    {
                        if (list[i].ActorId == actorId && list[i].AgentHandler == agentHandlerType)
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
                    ActorId = actorId,
                    AgentHandler = agentHandlerType,
                });
            }, false);
        }

        public void RemoveListener(long actorId, int evtType, string agentHandlerType)
        {
            lineActor.SendAsync(() =>
            {
                if (!eventHandlers.ContainsKey(evtType))
                    return;
                var list = eventHandlers[evtType];
                for (int i = list.Count - 1; i >= 0; --i)
                {
                    if (list[i].AgentHandler == agentHandlerType && list[i].ActorId == actorId)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }, false);
        }

        public void ClearListener(long actorId)
        {
            lineActor.SendAsync(() =>
            {
                var evtList = new List<int>(eventHandlers.Keys);
                foreach (var evt in evtList)
                {
                    var list = eventHandlers[evt];
                    for (int i = list.Count - 1; i >= 0; --i)
                    {
                        if (list[i].ActorId == actorId)
                        {
                            list.RemoveAt(i);
                            break;
                        }
                    }
                }
            }, false);
        }

        public void DispatchEvent(int evtType, Func<int, TActor, Type, Task<bool>> checkDispatchFunc, Param param = null)
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
                    var actor = await ActorManager.Get<TActor>(evtInfo.ActorId);
                    if (actor == null)
                        continue;

                    var info = evtInfo; //需要存个临时变量
                    _ = actor.SendAsync(async () =>
                    {
                        try
                        {
                            var handler = HotfixMgr.GetInstance<IEventListener>(info.AgentHandler);
                            var agentType = handler.GetType().BaseType.GenericTypeArguments[0];
                            if (agentType.GetInterface(typeof(IComponentActorAgent).FullName) != null)
                            {
                                //actor
                                if (checkDispatchFunc != null && !(await checkDispatchFunc(evtType, actor, null)))
                                    return;
                                await handler.InternalHandleEvent(actor.GetAgent(agentType), evt);
                            }
                            else if (agentType.GetInterface(typeof(IComponentAgent).FullName) != null)
                            {
                                //component
                                var compType = agentType.BaseType.GenericTypeArguments[0];
                                if (checkDispatchFunc != null && !(await checkDispatchFunc(evtType, actor, compType)))
                                    return;
                                var comp = await actor.GetComponent(compType);
                                await handler.InternalHandleEvent(actor.GetAgent(agentType), evt);
                            }
                        }
                        catch (Exception e)
                        {
                            LOGGER.Error(e.ToString());
                        }
                    }, false);
                }
            }, false);
        }
    }
}
