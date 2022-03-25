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
        public static async void AddListener(long entityId, int evtType, string agentType)
        {
            var entity = await EntityMgr.GetOrNewEntity(entityId);
            entity.EvtDispatcher.AddListener(evtType, agentType);
        }

        public static async void RemoveListener(long entityId, int evtType, string agentType)
        {
            var entity = await EntityMgr.GetOrNewEntity(entityId);
            entity.EvtDispatcher.RemoveListener(evtType, agentType);
        }

        public static async void DispatchEvent(long entityId, int evtType, Param param = null)
        {
            var entity = await EntityMgr.GetOrNewEntity(entityId);
            entity.EvtDispatcher.DispatchEvent(evtType, param);
        }

        public static async void ClearEvent(long entityId)
        {
            var entity = await EntityMgr.GetOrNewEntity(entityId);
            entity.EvtDispatcher.Clear();
        }



        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public long EntityID { get; }
        readonly Dictionary<int, List<string>> eventHandlers;
        public EventDispatcher(long entityId)
        {
            this.EntityID = entityId;
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
        public async void DispatchEvent(int evtType, Param param = null)
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
                //GetCompAgent中会检测调用链 async void会误导系统，需要重置
                //RuntimeContext.ResetContext();

                var list = eventHandlers[evtType];
                foreach (var handlerType in list)
                {
                    try
                    {
                        var handler = HotfixMgr.GetInstance<IEventListener>(handlerType);
                        var agentType = handler.GetType().BaseType.GenericTypeArguments[0];
                        var compAgent = await EntityMgr.GetCompAgent(EntityID, agentType);
                        _ = compAgent.Owner.Actor.SendAsync(() => {
                            return handler.InnerHandleEvent(compAgent, evt);
                        });
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