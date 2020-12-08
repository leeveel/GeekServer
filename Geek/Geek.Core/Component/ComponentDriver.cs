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
using Geek.Core.Actor;
using Geek.Core.Hotfix;
using Geek.Core.Events;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Geek.Core.CrossDay;

namespace Geek.Core.Component
{
    public class ComponentDriver
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        readonly ComponentActor owner;
        //只在driver访问
        readonly Dictionary<Type, DateTime> activeTimeMap = new Dictionary<Type, DateTime>();
        //在actor和driver线程都会访问
        readonly ConcurrentDictionary<Type, BaseComponent> activeCompMap = new ConcurrentDictionary<Type, BaseComponent>();
        public int ActiveCompNum => activeCompMap.Count;

        public ComponentDriver(ComponentActor actor)
        {
            owner = actor;
        }

        /// <summary>driver线程</summary>
        public async Task<BaseComponent> GetComponent(Type compType, bool doActive)
        {
            return await owner.GetLifeActor(compType).SendAsync(async () =>
            {
                activeCompMap.TryGetValue(compType, out var retComp);
                if (retComp == null)
                {
                    retComp = ComponentMgr.Singleton.NewComponent(owner, compType);
                    activeCompMap.TryAdd(compType, retComp);
                }
                if (doActive && !retComp.IsActive)
                {
                    await retComp.Active();
                    if (retComp.Agent != null)
                        await retComp.Agent.Active();
                }
                activeTimeMap[compType] = DateTime.Now;
                return retComp;
            });
        }

        /// <summary>actor线程</summary>
        public async Task InitListener()
        {
            var allComps = ComponentMgr.Singleton.GetAllCompsInfo(owner);
            foreach (var info in allComps)
            {
                if (!info.IsEventListener && !HotfixMgr.IsAgentInterface(info.CompType, typeof(IEventListener)))
                    continue;

                activeCompMap.TryGetValue(info.CompType, out var comp);
                if (comp == null)
                    comp = await GetComponent(info.CompType, false);

                try
                {
                    if (comp is IEventListener listener)
                        listener.InitEventListener();
                    if (comp.Agent is IEventListener listenerAgent)
                        listenerAgent.InitEventListener();
                }
                catch (Exception e)
                {
                    LOGGER.Error("com InitListener 异常:{} {}", owner.ActorId, comp.GetType());
                    LOGGER.Fatal(e.ToString());
                }
            }
        }

        public async Task DeactiveAllComps()
        {
            foreach(var kv in activeCompMap)
                await kv.Value.Deactive();
        }

        public async Task<bool> ReadyToDeactiveAllComps()
        {
            foreach (var kv in activeCompMap)
            {
                if (!await kv.Value.ReadyToDeactive())
                    return false;
            }
            return true;
        }

        /// <summary>actor线程</summary>
        public async Task CompCrossDay(int openServerDay)
        {
            //先获取所有跨天组件
            var allComps = ComponentMgr.Singleton.GetAllCompsInfo(owner);
            foreach (var info in allComps)
            {
                if (!info.IsCrossDay && !HotfixMgr.IsAgentInterface(info.CompType, typeof(ICrossDay)))
                    continue;

                await GetComponent(info.CompType, true);

                activeCompMap.TryGetValue(info.CompType, out var comp);
                try
                {
                    if (comp is ICrossDay crossDay)
                        await crossDay.OnCrossDay(openServerDay);
                    if (comp.Agent is ICrossDay crossDayAgent)
                        await crossDayAgent.OnCrossDay(openServerDay);
                }
                catch (Exception e)
                {
                    LOGGER.Error("com 跨天 异常:{} {}", owner.ActorId, comp.GetType());
                    LOGGER.Fatal(e.ToString());
                }
            }
        }

        /// <summary>actor线程</summary>
        public async Task CompTick(long deltaTime)
        {
            var allComps = ComponentMgr.Singleton.GetAllCompsInfo(owner);
            foreach (var info in allComps)
            {
                activeCompMap.TryGetValue(info.CompType, out var comp);
                if (comp == null || !comp.IsActive)
                    continue;
                if (!info.IsTick && !HotfixMgr.IsAgentInterface(info.CompType, typeof(ITick)))
                    continue;

                try
                {
                    if (comp is ITick ticker)
                        await ticker.OnTick(deltaTime);
                    if (comp.Agent is ITick tickerAgent)
                        await tickerAgent.OnTick(deltaTime);
                }
                catch (Exception e)
                {
                    LOGGER.Error("com tick 异常:{} {}", owner.ActorId, comp.GetType());
                    LOGGER.Fatal(e.ToString());
                }
            }
        }

        /// <summary>
        /// driver+actor线程
        /// 回存需要actor线程await执行
        /// </summary>
        public async Task RecycleIdleComps(List<Type> excludeList, float idleMinutes = 15f)
        {
            var now = DateTime.Now;
            var allComps = ComponentMgr.Singleton.GetAllCompsInfo(owner);
            foreach (var info in allComps)
            {
                var key = info.CompType;
                if (excludeList.Contains(key))
                    continue;
                if (!activeTimeMap.ContainsKey(key))
                    continue;

                activeCompMap.TryGetValue(key, out var comp);
                if (comp == null || !comp.IsActive)
                    continue;

                try
                {
                    //销毁长时间不活跃的模块
                    if ((now - activeTimeMap[key]).TotalMinutes > idleMinutes)
                    {
                        var compType = comp.GetType();
                        await owner.GetLifeActor(compType).SendAsync(async () =>
                        {
                            if ((now - activeTimeMap[key]).TotalMinutes > idleMinutes)
                            {
                                //理论上一定会返回true，因为销毁时长大于回存时间
                                if (await comp.ReadyToDeactive())
                                {
                                    await comp.Deactive();
                                    if (comp.Agent != null)
                                        await comp.Agent.Deactive();
                                    activeCompMap.TryRemove(key, out _);
                                    activeTimeMap.Remove(compType);
                                }
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    LOGGER.Error("com deactive 异常:{} {}", owner.ActorId, comp.GetType());
                    LOGGER.Fatal(e.ToString());
                }
            }
        }
    }
}