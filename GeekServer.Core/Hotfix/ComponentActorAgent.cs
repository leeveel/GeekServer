using System;
using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class ComponentActorAgent<T> : IComponentActorAgent where T : ComponentActor
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public T Actor => (T)Owner;
        public long ActorId => Actor.ActorId;
        public object Owner { get; set; }

        public Task ActiveAllComps()
        {
            return Actor.ActiveAllComps();
        }

        public Task<bool> IsCompActive<TComp>() where TComp : BaseComponent
        {
            return Actor.IsCompActive(typeof(TComp));
        }

        public async Task<TAgent> GetCompAgent<TAgent>() where TAgent : IComponentAgent, new()
        {
            var type = typeof(TAgent).BaseType;
            if (type.IsGenericType)
            {
                var comp = await Actor.GetComponent(type.GenericTypeArguments[0]);
                return comp.GetAgentAs<TAgent>();
            }
            return default;
        }

        public async Task<TAgent> GetCompAgent<TComp, TAgent>() where TComp : BaseComponent, new() where TAgent : IComponentAgent
        {
            return (await Actor.GetComponent<TComp>()).GetAgentAs<TAgent>();
        }

        public virtual Task Active()
        {
            return Task.CompletedTask;
        }

        public virtual Task Deactive()
        {
            return Task.CompletedTask;
        }

        /// <summary>延时单次回调</summary>
        public long DelayCall<TH>(long delay, Param param = null) where TH : ITimerHandler
        {
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal("不能添加hotfix工程的类型作为timer参数 DelayCall");
                return -1;
            }
            return QuartzTimer.AddDelay(delay, ActorId, GetType().FullName, typeof(TH).FullName, param);
        }

        /// <summary>定时周期回调</summary>
        public long AddTimer<TH>(long delay, long period, Param param = null) where TH : ITimerHandler
        {
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal("不能添加hotfix工程的类型作为timer参数 AddTimer");
                return -1;
            }
            return QuartzTimer.AddTimer(delay, period, ActorId, GetType().FullName, typeof(TH).FullName, param);
        }

        public Task SendAsync(Action work, bool checkLock = true, int timeOut = BaseActor.TIME_OUT)
        {
            return Actor.SendAsync(work, checkLock, timeOut);
        }

        public Task<TRet> SendAsync<TRet>(Func<TRet> work, bool checkLock = true, int timeOut = BaseActor.TIME_OUT)
        {
            return Actor.SendAsync(work, checkLock, timeOut);
        }

        public Task SendAsync(Func<Task> work, bool checkLock = true, int timeOut = BaseActor.TIME_OUT)
        {
            return Actor.SendAsync(work, checkLock, timeOut);
        }

        public Task<TRet> SendAsync<TRet>(Func<Task<TRet>> work, bool checkLock = true, int timeOut = BaseActor.TIME_OUT)
        {
            return Actor.SendAsync(work, checkLock, timeOut);
        }
    }
}