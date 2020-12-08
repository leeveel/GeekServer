/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using Base;
using System;
using Geek.Core.Actor;
using Geek.Core.Hotfix;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Core.Events
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

        BaseActor ownerActor;
        Dictionary<int, List<string>> eventHandlers;
        public EventDispatcher(BaseActor actor)
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
            Event evt = new Event();
            evt.EventId = evtType;
            evt.Data = param;

            if(eventHandlers.ContainsKey(evtType))
            {
                var list = eventHandlers[evtType];
                foreach (var agentType in list)
                {
                    var agent = HotfixMgr.GetInstance<IEventHandler>(agentType);
                    if (agent != null)
                        Task.Run(() => ownerActor.SendAsync(async () => { await agent.HandleEvent(ownerActor, evt); }));
                    else
                        LOGGER.Error("DispatchEvent create agent failed:" + agentType);
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