using System;
using System.Collections.Generic;

namespace Geek.Client
{
    public struct Event
    {
        public static Event NULL = new Event();
        public int EventId;
        public object Data;
    }
    
    public class EventDispatcher
    {
        private class EvtHandInfo
        {
            public Action handler1;
            public Action<Event> handler2;
        }

        /// <summary>
        /// C#事件
        /// </summary>
        private Dictionary<Int32, EvtHandInfo> mEventHandlers;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="owner">事件分派器所有者</param>
        public EventDispatcher()
        {
            mEventHandlers = new Dictionary<Int32, EvtHandInfo>();
        }

        public void addListener(Int32 evtType, Action handler)
        {
            if (!mEventHandlers.ContainsKey(evtType))
                mEventHandlers[evtType] = new EvtHandInfo();
            var info = mEventHandlers[evtType];
            info.handler1 += handler;
        }

        /// <summary>
        /// 添加一个事件监听
        /// </summary>
        /// <param name="evtType">监听的事件类型</param>
        /// <param name="handler">回调处理</param>
        public void addListener(Int32 evtType, Action<Event> handler)
        {
            if (!mEventHandlers.ContainsKey(evtType))
                mEventHandlers[evtType] = new EvtHandInfo();
            var info = mEventHandlers[evtType];
            info.handler2 += handler;
        }

        /// <summary>
        /// 移除一个事件监听
        /// </summary>
        /// <param name="evtType"></param>
        /// <param name="handler"></param>
        public void removeListener(Int32 evtType, Action handler)
        {
            if (!mEventHandlers.ContainsKey(evtType))
                return;
            var info = mEventHandlers[evtType];
            info.handler1 -= handler;
        }

        /// <summary>
        /// 移除一个事件监听
        /// </summary>
        /// <param name="evtType"></param>
        /// <param name="handler"></param>
        public void removeListener(Int32 evtType, Action<Event> handler)
        {
            if (!mEventHandlers.ContainsKey(evtType))
                return;
            var info = mEventHandlers[evtType];
            info.handler2 -= handler;
        }

        public void removeListeners(Int32 evtType)
        {
            if (mEventHandlers.ContainsKey(evtType))
                mEventHandlers.Remove(evtType);
        }

        /// <summary>
        /// 分发事件
        /// </summary>
        /// <param name="evtType">Evt type.</param>
        /// <param name="parameter">Parameter.</param> 
        public void dispatchEvent(int evtType, object parameter = null)
        {
            Event evt = new Event();
            evt.EventId = evtType;
            evt.Data = parameter;
            dispatchEvent(evt);
        }

        public void dispatchEvent(BaseEventID evtType, object param = null)
        {
            Event evt = new Event();
            evt.EventId = (int)evtType;
            evt.Data = param;
            dispatchEvent(evt);
        }

        /// <summary>
        /// 分派事件
        /// </summary>
        /// <param name="evt"></param>
        public void dispatchEvent(Event evt)
        {
            try
            {
                handleEvent(evt);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"evtId={evt.EventId} {e.ToString()}");
            }
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="evt"></param>  
        private void handleEvent(Event evt)
        {
            var evtId = evt.EventId;
            if (!mEventHandlers.ContainsKey(evt.EventId))
                return;
            var info = mEventHandlers[evtId];
            if (info.handler1 != null)
                info.handler1();
            if (info.handler2 != null)
                info.handler2(evt);
        }
        
        /// <summary>
        /// 清除所有未分发的事件
        /// </summary>
        public void clear()
        {
            mEventHandlers.Clear();
        }
    }
}