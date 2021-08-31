using System;
using System.Collections.Generic;

namespace Geek.Server
{
    public struct Event
    {
        public static Event NULL = new Event();
        public int EventId;
        public Param Data;
    }
    
    public class EventDispatcher
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public ComponentActor ownerActor { get; }
        readonly Dictionary<int, List<string>> eventHandlers;
        public EventDispatcher(ComponentActor actor)
        {
            this.ownerActor = actor;
            eventHandlers = new Dictionary<int, List<string>>();
        }

        public void AddListener(int evtType, string agentType)
        {
            if (!eventHandlers.ContainsKey(evtType))
                eventHandlers.Add(evtType, new List<string>());
            if(!eventHandlers[evtType].Contains(agentType))
                eventHandlers[evtType].Add(agentType);
        }

        /// <summary>
        /// 移除一个事件监听
        /// </summary>
        public void RemoveListener(int evtType, string agentType)
        {
            if (eventHandlers.ContainsKey(evtType))
            {
                if (eventHandlers[evtType].Contains(agentType))
                    eventHandlers[evtType].Remove(agentType);
            }
        }

        /// <summary>
        /// 分发事件
        /// </summary>
        public void DispatchEvent(int evtType, Param param = null)
        {
            if(HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal($"不能添加hotfix工程的类型作为参数 DispatchEvent {param.GetType()}");
                return;
            }

            Event evt = new Event();
            evt.EventId = evtType;
            evt.Data = param;

            if(eventHandlers.ContainsKey(evtType))
            {
                var list = eventHandlers[evtType];
                foreach (var handlerType in list)
                {
                    try
                    {
                        var handler = HotfixMgr.GetInstance<IEventListener>(handlerType);
                        var agentType = handler.GetType().BaseType.GenericTypeArguments[0];
                        //component
                        var compType = agentType.BaseType.GenericTypeArguments[0];
                        ownerActor.SendAsync(async () => {
                            var comp = await ownerActor.GetComponent(compType);
                            await handler.InternalHandleEvent(comp.GetAgent(agentType), evt);
                        }, false);
                    }
                    catch (Exception e)
                    {
                        LOGGER.Error(e.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 清除所有未分发的事件
        /// </summary>
        public void Clear()
        {
            eventHandlers.Clear();
        }
    }
}