using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Geek.Server
{
    public class ComponentDriver
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        readonly ComponentActor owner;
        readonly ConcurrentDictionary<Type, DateTime> activeTimeMap = new ConcurrentDictionary<Type, DateTime>();
        //在actor和driver线程都会访问
        readonly ConcurrentDictionary<Type, BaseComponent> activeCompMap = new ConcurrentDictionary<Type, BaseComponent>();
        public int ActiveCompNum => activeCompMap.Count;

        public ComponentDriver(ComponentActor actor)
        {
            owner = actor;
        }

        /// <summary>线程安全</summary>
        public async Task<BaseComponent> GetComponent(Type compType)
        {
            //当comp已激活,且肯定不满足回收条件时可以不用入lifeactor线程
            var now = DateTime.Now;
            float threshold = Settings.Ins.CompRecycleTime * 0.67f;//0.67 ~= 2/3
            if (activeTimeMap.TryGetValue(compType, out var activeTime)
                && (now - activeTime).TotalMinutes < threshold
                && activeCompMap.TryGetValue(compType, out var retComp))
            {
                activeTimeMap[compType] = DateTime.Now;
                return retComp;
            }

            //comp的active不一定在所属actor线程执行，谁来获取就在谁的线程执行
            return await owner.GetLifeActor(compType).SendAsync(async () =>
            {
                activeCompMap.TryGetValue(compType, out var retComp);
                if (retComp == null)
                {
                    retComp = ComponentMgr.Singleton.NewComponent(owner, compType);
                    activeCompMap.TryAdd(compType, retComp);
                }
                if (!retComp.IsActive)
                {
                    await retComp.Active();
                    if (retComp.GetAgent() != null)
                        await retComp.GetAgent().Active();
                }
                activeTimeMap[compType] = DateTime.Now;
                return retComp;
            });
        }

        public Task<bool> IsCompActive(Type compType)
        {
            activeCompMap.TryGetValue(compType, out var comp);
            var ret = comp != null && comp.IsActive;
            return Task.FromResult(ret);
        }

        /// <summary>清除所有comp的agent(热更时)</summary>
        public void ClearAllCompsAgent()
        {
            foreach (var kv in activeCompMap)
                kv.Value.ClearCacheAgent();
        }

        /// <summary>actor线程执行</summary>
        public async Task InitListener()
        {
            var list = HotfixMgr.GetEventListeners(owner.GetAgent().GetType());
            if (list == null)
                return;
            foreach(var listener in list)
                await listener.InnerInitListener(owner.GetAgent());
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
                var compType = info.CompType;
                if (excludeList.Contains(compType))
                    continue;
                if (!activeTimeMap.ContainsKey(compType))
                    continue;

                activeCompMap.TryGetValue(compType, out var comp);
                if (comp == null || !comp.IsActive)
                    continue;

                try
                {
                    //销毁长时间不活跃的模块
                    if (activeTimeMap.TryGetValue(compType, out var activeTime) && (now - activeTime).TotalMinutes > idleMinutes)
                    {
                        await owner.GetLifeActor(compType).SendAsync(async () =>
                        {
                            if (activeTimeMap.TryGetValue(compType, out var activeTime) && (now - activeTime).TotalMinutes > idleMinutes)
                            {
                                //理论上一定会返回true，因为销毁时长大于回存时间
                                if (await comp.ReadyToDeactive())
                                {
                                    await comp.Deactive();
                                    if (comp.GetAgent() != null)
                                        await comp.GetAgent().Deactive();
                                    activeCompMap.TryRemove(compType, out _);
                                    activeTimeMap.TryRemove(compType, out _);
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