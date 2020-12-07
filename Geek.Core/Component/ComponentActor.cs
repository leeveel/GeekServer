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
using System.Threading.Tasks;
using System.Collections.Generic;
using Geek.Core.Hotfix;
using Geek.Core.Timer;
using Geek.Core.CrossDay;

namespace Geek.Core.Component
{
    public abstract class ComponentActor : BaseActor
    {
        /// <summary>常驻内存组件</summary>
        public readonly List<Type> ConstCompTypeList = new List<Type>();
        /// <summary>actor/组件自动回收(玩家actor下线时开启，其他actor不开)</summary>
        public bool AutoRecycleEnable { get; set; }
        /// <summary>是否自动跨天[ActorManager驱动]</summary>
        public bool AutoCrossDay { get; set; } = true;
        /// <summary>其他服务器的Actor则只读</summary>
        public bool ReadOnly { get; set; }

        ComponentDriver driver;
        public ScheduleTimer Timer { get; private set; }

        public ComponentActor()
        {
            Timer = new ScheduleTimer(this);
            driver = new ComponentDriver(this);
        }

        readonly Dictionary<Type, WorkerActor> LifeDic = new Dictionary<Type, WorkerActor>();

        public WorkerActor GetLifeActor(Type compType)
        {
            lock (LifeDic)
            {
                LifeDic.TryGetValue(compType, out var lifeActor);
                if (lifeActor == null)
                {
                    lifeActor = new WorkerActor();
                    LifeDic[compType] = lifeActor;
                }
                return lifeActor;
            }
        }

        public IComponentActorAgent Agent => HotfixMgr.GetAgent<IComponentActorAgent>(this);
        public T GetAgentAs<T>() where T : IComponentActorAgent { return (T)Agent; }

        public override Task Active()
        {
            return Task.CompletedTask;
        }

        public override Task Deactive()
        {
            return driver.DeactiveAllComps();
        }

        public override Task<bool> ReadyToDeactive()
        {
            if (!AutoRecycleEnable)
                return Task.FromResult(false);
            if (ConstCompTypeList.Count != driver.ActiveCompNum)
                return Task.FromResult(false);

            return driver.ReadyToDeactiveAllComps();
        }

        /// <summary>
        /// 获取actor身上的Component,线程安全
        /// </summary>
        public async Task<T> GetComponent<T>() where T : BaseComponent, new()
        {
            return (T)(await driver.GetComponent(typeof(T), true));
        }

        /// <summary>
        /// 获取actor身上的Component,线程安全
        /// </summary>
        public Task<BaseComponent> GetComponent(Type compType)
        {
            return driver.GetComponent(compType, true);
        }

        /// <summary>
        /// actor永久消失，清除actor数据库数据，比如公会解散，玩家清档等
        /// </summary>
        public void Die()
        {
            SendAsync(async () =>
            {
                if (Agent is IDeadable deadable)
                    await deadable.Die();

                ReadOnly = true;
                ConstCompTypeList.Clear();
                await Deactive();
                await ActorManager.Remove(ActorId, false);
                var list = ComponentMgr.Singleton.GetAllComps(this);
                var die = new DeadComp();
                die.Init(this);
                await die.Active();
                await die.Die(list);
            });
        }

        public async Task Tick(long deltaTime)
        {
            if (ReadOnly)
                return;
            if (this is ITick ticker)
                await ticker.OnTick(deltaTime);
            if (Agent is ITick tickerAgent)
                await tickerAgent.OnTick(deltaTime);
            await Timer.Tick(deltaTime);
            await driver.CompTick(deltaTime);
            if (AutoRecycleEnable)
                await driver.RecycleIdleComps(ConstCompTypeList, Settings.Ins.compRecycleTime);
        }

        bool listenerInited;
        public Task InitListener()
        {
            if (ReadOnly || listenerInited)
                return Task.CompletedTask;
            listenerInited = true;
            return driver.InitListener();
        }

        public async Task CrossDay(int openServerDay)
        {
            if (ReadOnly)
                return;
            if (this is ICrossDay crossDay)
                await crossDay.OnCrossDay(openServerDay);
            if (this.Agent is ICrossDay agentCrossDay)
                await agentCrossDay.OnCrossDay(openServerDay);
            await driver.CompCrossDay(openServerDay);
        }
    }
}