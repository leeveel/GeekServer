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
using Geek.Core.Component;
using System.Collections.Generic;
using System.Threading.Tasks;
using Geek.Core.Hotfix;

namespace Geek.Core.Events
{
    public class CrossActorEventDispatcher<TActor> where TActor : ComponentActor, new()
    {
        public class EventInfo
        {
            public long ActorId;
            public string AgentCallbackType;
        }


        WorkerActor lineActor = new WorkerActor();
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        readonly Dictionary<int, List<EventInfo>> eventHandlers = new Dictionary<int, List<EventInfo>>();

        public void AddListener(long actorId, int evtType, string agentHandlerType)
        {
            lineActor.SendAsync(() => {
                if (eventHandlers.ContainsKey(evtType))
                {
                    var list = eventHandlers[evtType];
                    for (int i = list.Count - 1; i >= 0; --i)
                    {
                        if (list[i].ActorId == actorId && list[i].AgentCallbackType == agentHandlerType)
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
                    AgentCallbackType = agentHandlerType,
                });
            });
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
                    if (list[i].AgentCallbackType == agentHandlerType && list[i].ActorId == actorId)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            });
        }

        public void ClearListener(long actorId)
        {
            lineActor.SendAsync(() =>
            {
                var evtList = new List<int>(eventHandlers.Keys);
                foreach(var evt in evtList)
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
            });
        }

        public void DispatchEvent(int evtType, Func<TActor, Task<bool>> checkDispatchFunc, Param param = null)
        {
            lineActor.SendAsync(async () =>
            {
                if (!eventHandlers.ContainsKey(evtType))
                    return;

                Event evt = new Event();
                evt.EventId = evtType;
                evt.Data = param;
                var list = eventHandlers[evtType];
                foreach(var info in list)
                {
                    var actor = await ActorManager.Get<TActor>(info.ActorId);
                    if (actor == null)
                        continue;

                    _ = Task.Run(()=> {
                        _ = actor.SendAsync(async () => {
                            if (checkDispatchFunc != null && !(await checkDispatchFunc(actor)))
                                return;

                            var agent = HotfixMgr.GetInstance<IEventHandler>(info.AgentCallbackType);
                            if (agent != null)
                                await agent.HandleEvent(actor, evt);
                            else
                                LOGGER.Error("CrossActorEventDispatcher create agent failed:" + info.AgentCallbackType);
                        });
                    });
                }
            });
        }
    }
}
