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
using System;
using Geek.Core.Component;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.Generic;
using Geek.Core.CrossDay;

namespace Geek.Core.Actor
{
    public static class ActorManager
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        readonly static ConcurrentDictionary<long, ComponentActor> actorMap = new ConcurrentDictionary<long, ComponentActor>();
        readonly static Dictionary<long, DateTime> activeTimeMap = new Dictionary<long, DateTime>();
        private readonly static Dictionary<long, WorkerActor> lifeActorDic = new Dictionary<long, WorkerActor>();
        private static ICrossDayTrigger CrossDayTrigger { get; set; }

        private static WorkerActor GetLifeActor(long actorId)
        {
            lock (lifeActorDic)
            {
                lifeActorDic.TryGetValue(actorId, out var actor);
                if (actor == null)
                {
                    actor = new WorkerActor();
                    lifeActorDic.Add(actorId, actor);
                }
                return actor;
            }
        }

        public static Task<T> Get<T>(long id) where T : ComponentActor
        {
            return GetLifeActor(id).SendAsync(() => {
                //此接口允许actor返回空
                //所以不更新访问时间[回存和链接不需要更新访问时间]
                actorMap.TryGetValue(id, out var actor);
                if (!(actor is T))
                {
                    LOGGER.Error("actor 类型不匹配>get:{}, org:{} id:{}", typeof(T), actor == null ? "null" : actor.GetType().FullName, id);
#if DEBUG
                    throw new Exception("actor GetOrNew类型转换错误");
#endif
                }
                return (T)actor;
            });
        }

        public static async Task<T> GetOrNew<T>(long id) where T : ComponentActor, new()
        {
            return await GetLifeActor(id).SendAsync(async () =>
            {
                actorMap.TryGetValue(id, out var actor);
                if (actor == null)
                {
                    actor = new T()
                    {
                        ActorId = id
                    };
                    await actor.Active();
                    if (actor.Agent != null)
                        await actor.Agent.Active();
                    await actor.InitListener();
                    actorMap.TryAdd(id, actor);
                    if (actor is ICrossDayTrigger trigger)
                    {
                        if (CrossDayTrigger != null)
                            LOGGER.Warn("设置了多个ICrossDayTrigger,仅能有一个");
                        CrossDayTrigger = trigger;
                    }
                }
                activeTimeMap[id] = DateTime.Now;
                return (T)actor;
            });
        }

        public static Task<bool> Remove(long id, bool checkActiveTime = true)
        {
            return GetLifeActor(id).SendAsync(() =>
            {
                if(!checkActiveTime
                || !activeTimeMap.ContainsKey(id) 
                || (DateTime.Now - activeTimeMap[id]).TotalMinutes > Settings.Ins.actorRecycleTime)
                {
                    if(actorMap.TryRemove(id, out _))
                    {
                        activeTimeMap.Remove(id);
                        return true;
                    }
                }
                return false;
            });
        }

        /// <summary>关服时调用</summary>
        public static async Task RemoveAll()
        {
            //可能是mongodb超连接数异常退出程序
            //延时等待mongdb连接数空闲
            await Task.Delay(500);
            int count = 0;
            foreach (var kv in actorMap)
            {
                var actor = kv.Value;
                try
                {
                    await actor.SendAsync(actor.Deactive);
                    if(actor.Agent != null)
                        await actor.SendAsync(actor.Agent.Deactive);
                    count++;
                }
                catch (Exception e)
                {
                    LOGGER.Error("actor deactive 异常:" + actor.ActorId + " " + actor.GetType());
                    LOGGER.Fatal(e.ToString());
                }
            }
            actorMap.Clear();
            LOGGER.Info("deactive all actors num=" + count);
            Console.WriteLine("deactive all actors num=" + count);
        }

        /// <summary>
        /// 仅仅在主线程调用
        /// </summary>
        public static Task Tick(long deltaTime)
        {
            foreach (var kv in actorMap)
            {
                var actorId = kv.Key;
                var actor = kv.Value;
                _ = actor.SendAsync(async () => {
                    //Actor回收
                    if ((DateTime.Now - activeTimeMap[actorId]).TotalMinutes > 30)
                    {
                        await GetLifeActor(actorId).SendAsync(async () =>
                        {
                            if (activeTimeMap.ContainsKey(actorId)
                            && (DateTime.Now - activeTimeMap[actorId]).TotalMinutes > 30)
                            {
                                if (await actor.ReadyToDeactive())
                                {
                                    actorMap.TryRemove(actorId, out var act);
                                    activeTimeMap.Remove(actorId);
                                    await act.Deactive();
                                }
                            }
                        });
                    }
                    //Tick
                    try
                    {
                        await actor.Tick(deltaTime);
                    }
                    catch (Exception e)
                    {
                        LOGGER.Error("actor tick 异常:" + actor.ActorId, " " + actor.GetType());
                        LOGGER.Fatal(e.ToString());
                    }
                });
            }
            //跨天判断
            _ = CheckCrossDay();
            return Task.CompletedTask;
        }

        private static async Task CheckCrossDay()
        {
            if (CrossDayTrigger != null)
            {
                int openServerDay = await CrossDayTrigger.CheckCrossDay();
                if (openServerDay < 0)
                    return;
                foreach (var kk in actorMap)
                {
                    var inActor = kk.Value;
                    if (inActor == CrossDayTrigger)
                        continue;
                    if (!inActor.AutoCrossDay)
                        continue;
                    _ = inActor.SendAsync(async () => {
                        await inActor.CrossDay(openServerDay);
                    });
                }
            }
        }

    }
}
