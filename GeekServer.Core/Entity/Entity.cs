using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Geek.Server
{
    public sealed class Entity
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        long entityId;
        int entityType;
        internal bool ReadOnly;
        internal bool AutoCrossDay = true;
        internal bool AutoRecyleEnable;
        internal EventDispatcher EvtDispatcher;
        //在actor和driver线程都会访问
        readonly ConcurrentDictionary<Type, BaseComponent> activeCompMap = new ConcurrentDictionary<Type, BaseComponent>();
        readonly ConcurrentDictionary<Type, DateTime> activeTimeMap = new ConcurrentDictionary<Type, DateTime>();

        WorkerActor workerActor;
        Dictionary<Type, WorkerActor> workerDic;
        private bool isShareActor = true;

        internal Entity(int entityType, long entityId)
        {
            this.entityId = entityId;
            this.entityType = entityType;
            EvtDispatcher = new EventDispatcher(entityId);
            if (CompSetting.Singleton.IsEntityShareActor(entityType))
            {
                isShareActor = true;
                workerActor = new WorkerActor() { entityType = entityType };
            }
            else
            {
                workerDic = new Dictionary<Type, WorkerActor>();
                var list = CompSetting.Singleton.GetAllComps(entityType);
                for (int i = 0; i < list.Count; i++)
                {
                    workerDic.Add(list[i], new WorkerActor() { entityType = entityType, compType = list[i] });
                }
            }
        }

        WorkerActor GetActor(Type compType)
        {
            if (isShareActor)
            {
                return workerActor;
            }
            else
            {
                workerDic.TryGetValue(compType, out WorkerActor actor);
                return actor;
            }
        }

        public async Task<T> GetCompAgent<T>() where T : BaseComponent
        {
            return (T)(await GetCompAgent(typeof(T)));
        }

        public async Task<IComponentAgent> GetCompAgent(Type compType)
        {
            var comp = await GetComponent(compType);
            return comp.GetAgent(compType);
        }

        async Task<BaseComponent> GetComponent(Type compType)
        {
            if (CompSetting.Singleton.IsAutoActive(entityType)
                && activeCompMap.TryGetValue(compType, out var comp))
            {
                activeTimeMap[compType] = DateTime.Now;
                return comp;
            }
            else
            {
                //当comp已激活,且肯定不满足回收条件时可以不用入线程
                var now = DateTime.Now;
                float threshold = Settings.Ins.CompRecycleTime * 0.7f;
                if (activeTimeMap.TryGetValue(compType, out var activeTime)
                    && (now - activeTime).TotalMinutes < threshold
                    && activeCompMap.TryGetValue(compType, out var retComp))
                {
                    activeTimeMap[compType] = DateTime.Now;
                    return retComp;
                }
            }
            var worker = GetActor(compType);
            long callChainId = worker.IsNeedEnqueue();
            if (callChainId < 0)
                return await ActiveComp(compType, worker);
            else
                return await worker.Enqueue(() => ActiveComp(compType, worker), callChainId);
        }

        private async Task<BaseComponent> ActiveComp(Type compType, WorkerActor actor)
        {
            var got = activeCompMap.TryGetValue(compType, out var retComp);
            if (retComp == null)
                retComp = CompSetting.Singleton.NewComponent(actor, entityType, entityId, compType);
            if (retComp == null)//没有注册Comp
                return retComp;
            if (!retComp.IsActive)
            {
                await retComp.Active();
                if (retComp.GetAgent() != null)
                    await retComp.GetAgent().Active();
            }
            if (!got)
                activeCompMap.TryAdd(compType, retComp);
            activeTimeMap[compType] = DateTime.Now;
            return retComp;
        }

        /// <summary>
        /// actor永久消失，清除actor数据库数据，比如公会解散，玩家清档等
        /// </summary>
        public async Task Die()
        {
            ReadOnly = true;
            var infoList = CompSetting.Singleton.GetAllCompsInfo(entityType);
            foreach (var info in infoList)
            {
                if (HotfixMgr.IsAgentInterface(info.CompType, typeof(IDeadable)))
                {
                    await GetActor(info.CompType).SendAsync(async () => {
                        activeCompMap.TryGetValue(info.CompType, out var comp);
                        if (comp == null)
                            comp = await GetComponent(info.CompType);
                        if (comp.GetAgent() is IDeadable deadAgent)
                            await deadAgent.Dieout();
                    });
                }
            }

            await Deactive();
            await EntityMgr.RemoveEntity(entityId);
            await deleteStates();
        }

        Task deleteStates()
        {
            var taskList = new List<Task>();
            var list = CompSetting.Singleton.GetAllComps(entityType);
            foreach (var t in list)
            {
                if (t.GetInterface(typeof(IState).FullName) == null)
                    continue;

                //删除数据库中所有StateComponent的document
                var arr = t.BaseType.GetGenericArguments();
                if (arr.Length <= 0)
                    continue;
                var sType = arr[0];
                if (!sType.IsSubclassOf(typeof(DBState)))
                    continue;

                var db = MongoDBConnection.Singleton.CurDateBase;
                var col = db.GetCollection<BsonDocument>(sType.FullName);
                var filter = Builders<BsonDocument>.Filter.Eq(MongoField.Id, entityId);
                taskList.Add(col.DeleteOneAsync(filter));
            }
            return Task.WhenAll(taskList);
        }

        public Task CrossDay(int openServerDay)
        {
            if (ReadOnly)
                return Task.CompletedTask;
            //先获取所有跨天组件
            var allComps = CompSetting.Singleton.GetAllCompsInfo(entityType);

            foreach (var info in allComps)
            {
                if (!HotfixMgr.IsAgentInterface(info.CompType, typeof(ICrossDay)))
                    continue;

                _ = GetActor(info.CompType).SendAsync(async () => {
                    try
                    {
                        activeCompMap.TryGetValue(info.CompType, out var comp);
                        if (comp == null)
                            comp = await GetComponent(info.CompType);
                        if (comp.GetAgent() is ICrossDay crossDayAgent)
                            await crossDayAgent.OnCrossDay(openServerDay);
                    }
                    catch (Exception e)
                    {
                        LOGGER.Error("comp 跨天 异常:{} {}", entityId, info.CompType);
                        LOGGER.Fatal(e.ToString());
                    }
                });
            }
            return Task.CompletedTask;
        }

        public async Task ActiveAutoActiveComps()
        {
            var list = CompSetting.Singleton.GetAllCompsInfo(entityType);
            foreach (var info in list)
            {
                if(info.AutoActive)
                    await GetComponent(info.CompType);
            }
        }

        public Task CompleteAllTask()
        {
            var taskList = new List<Task>();
            foreach (var kv in activeCompMap)
            {
                var task = kv.Value.Actor.SendAsync(() => Task.CompletedTask);
                taskList.Add(task);
            }
            return Task.WhenAll(taskList);
        }

        public Task Deactive()
        {
            foreach (var kv in activeCompMap)
                _ = kv.Value.Actor.SendAsync(kv.Value.Deactive);
            HotfixMgr.RemoveAgentCache(this);
            return Task.CompletedTask;
        }

        public void ClearAllCompsAgent()
        {
            foreach (var kv in activeCompMap)
            {
                var comp = kv.Value;
                comp.Actor.SendAsync(comp.ClearCacheAgent);
            }
        }

        public Task<bool> IsCompActive(Type compType)
        {
            activeCompMap.TryGetValue(compType, out var comp);
            var ret = comp != null && comp.IsActive;
            return Task.FromResult(ret);
        }

        public async Task InitListener()
        {
            var list = HotfixMgr.GetEventListeners(entityType);
            if (list == null)
                return;
            foreach (var listener in list)
                await listener.InitListener(entityId);
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

        public Task<bool> ReadyToDeactive()
        {
            return Task.FromResult(activeCompMap.Count <= 0);
        }

        /// <summary>
        /// driver+actor线程
        /// 回存需要actor线程await执行
        /// </summary>
        public async Task CheckIdle()
        {
            //自动激活的entity不自动回收
            if (CompSetting.Singleton.IsAutoActive(entityType))
                return;

            var idleMinutes = Settings.Ins.CompRecycleTime;
            var now = DateTime.Now;
            var allComps = CompSetting.Singleton.GetAllCompsInfo(entityType);
            foreach (var info in allComps)
            {
                var compType = info.CompType;
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
                        await GetActor(compType).SendAsync(async () =>
                        {
                            if (activeTimeMap.TryGetValue(compType, out var activeTime) && (now - activeTime).TotalMinutes > idleMinutes)
                            {
                                //理论上一定会返回true，因为销毁时长大于回存时间
                                if (await comp.ReadyToDeactive())
                                {
                                    await comp.Deactive();
                                    activeCompMap.TryRemove(compType, out _);
                                    activeTimeMap.TryRemove(compType, out _);
                                }
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    LOGGER.Error("com deactive 异常:{} {}", entityType, comp.GetType());
                    LOGGER.Fatal(e.ToString());
                }
            }
        }
    }
}
