using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Geek.Server
{
    public class ComponentActor : BaseActor
    {
        protected ComponentDriver driver;

        public int ActorType { get; }
        public EventDispatcher EvtDispatcher { get; }
        /// <summary>常驻内存组件</summary>
        public readonly List<Type> ConstCompTypeList = new List<Type>();
        /// <summary>actor/组件自动回收(玩家actor下线时开启，其他actor不开)</summary>
        public bool AutoRecycleEnable { get; set; }
        /// <summary>readOnly=true则只会从数据库读不会回存(比如读取其他服数据)</summary>
        public bool ReadOnly { get; set; }

        public ComponentActor(int actorType)
        {
            ActorType = actorType;
            EvtDispatcher = new EventDispatcher(this);
            driver = new ComponentDriver(this);
        }

        readonly ConcurrentDictionary<Type, WorkerActor> LifeDic = new ConcurrentDictionary<Type, WorkerActor>();
        internal WorkerActor GetLifeActor(Type compType)
        {
            LifeDic.TryGetValue(compType, out var lifeActor);
            lock (LifeDic)
            {
                LifeDic.TryGetValue(compType, out lifeActor);
                if (lifeActor == null)
                {
                    lifeActor = new WorkerActor();
                    LifeDic[compType] = lifeActor;
                }
            }
            return lifeActor;
        }

        ///<summary>清除缓存的agent(热更时)</summary>
        public void ClearCacheAgent()
        {
            driver.ClearAllCompsAgent();
        }

        public Task Active()
        {
            return ActiveAutoComps();
        }

        public async Task Deactive()
        {
            await driver.DeactiveAllComps();
            HotfixMgr.RemoveAgentCache(this);
        }

        /// <summary>
        /// 激活所有已注册的comp
        /// </summary>
        public async Task ActiveAllComps()
        {
            var list = ComponentMgr.Singleton.GetAllComps(this);
            foreach (var type in list)
                await GetComponent(type);
        }

        /// <summary>
        /// 激活自动激活组件
        /// </summary>
        /// <returns></returns>
        public async Task ActiveAutoComps()
        {
            var list = ComponentMgr.Singleton.GetAutoActiveComps(this);
            foreach (var type in list)
            {
                ConstCompTypeList.Add(type);
                await GetComponent(type);
            }
        }

        /// <summary>
        /// actor是否可以回收了
        /// </summary>
        public override Task<bool> ReadyToDeactive()
        {
            if (!AutoRecycleEnable)
                return Task.FromResult(false);
            if (ConstCompTypeList.Count < driver.ActiveCompNum)
                return Task.FromResult(false);

            return driver.ReadyToDeactiveAllComps();
        }

        public Task<bool> IsCompActive(Type compType)
        {
            return driver.IsCompActive(compType);
        }

        /// <summary>
        /// 获取actor身上的Component,线程安全
        /// </summary>
        public async Task<T> GetComponent<T>() where T : BaseComponent, new()
        {
            return (T)(await driver.GetComponent(typeof(T)));
        }

        /// <summary>
        /// 获取actor身上的Component,线程安全
        /// </summary>
        public Task<BaseComponent> GetComponent(Type compType)
        {
            return driver.GetComponent(compType);
        }

        /// <summary>
        /// 获取组件Agent
        /// </summary>
        /// <typeparam name="TAgent"></typeparam>
        /// <returns></returns>
        public async Task<TAgent> GetCompAgent<TAgent>() where TAgent : IComponentAgent, new()
        {
            var type = typeof(TAgent).BaseType;
            if (type.IsGenericType)
            {
                //Agent是不能被二次继承的,所以直接取泛型参数是安全的
                var comp = await GetComponent(type.GenericTypeArguments[0]);
                return comp.GetAgentAs<TAgent>();
            }
            return default;
        }

        /// <summary>
        /// actor永久消失，清除actor数据库数据，比如公会解散，玩家清档等
        /// </summary>
        public async Task Dieout()
        {
            ReadOnly = true;
            ConstCompTypeList.Clear();
            await Deactive();
            await ActorManager.Remove(ActorId, false);
            var list = ComponentMgr.Singleton.GetAllComps(this);
            var die = new DieoutComp();
            die.Init(this);
            await die.Active();
            await die.Dieout(list);
        }

        /// <summary>
        /// 回收组件
        /// </summary>
        internal async Task CheckIdle()
        {
            if (AutoRecycleEnable)
                await driver.RecycleIdleComps(ConstCompTypeList, Settings.Ins.CompRecycleTime);
        }

        bool listenerInited;
        public Task InitListener()
        {
            if (ReadOnly || listenerInited)
                return Task.CompletedTask;
            listenerInited = true;
            return driver.InitListener();
        }
    }
}