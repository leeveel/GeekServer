using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class FuncComponentAgent<TComp> : BaseComponentAgent<TComp> where TComp : FuncComponent
    {
    }

    public abstract class BaseComponentAgent<TComp> : IComponentAgent where TComp : BaseComponent
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public TComp Comp => (TComp)Owner;
        public long ActorId => Actor.ActorId;
        public ComponentActor Actor => Comp.Actor;
        public object Owner { get; set; }

        /// <summary>
        /// 直接继承ComponentAgent<T>的可以通过此接口获取
        /// </summary>
        public async Task<TAgent> GetCompAgent<TAgent>() where TAgent : IComponentAgent
        {
            var type = typeof(TAgent).BaseType;
            if (type.IsGenericType)
            {
                var comp = await Actor.GetComponent(type.GenericTypeArguments[0]);
                return comp.GetAgentAs<TAgent>();
            }
            return default;
        }

        public virtual Task Active()
        {
            return Task.CompletedTask;
        }

        /// <summary>延时单次回调</summary>
        public long DelayCall<TH>(long delay, Param param = null) where TH : ITimerHandler
        {
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal($"不能添加hotfix工程的类型作为timer参数 DelayCall {typeof(TH)} {param.GetType()}");
                return -1;
            }
            return QuartzTimer.AddDelay(delay, ActorId, typeof(TH).FullName, param);
        }

        /// <summary>定时周期回调</summary>
        public long AddTimer<TH>(long delay, long period, Param param = null) where TH : ITimerHandler
        {
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal($"不能添加hotfix工程的类型作为timer参数 AddTimer {typeof(TH)} {param.GetType()}");
                return -1;
            }
            return QuartzTimer.AddTimer(delay, period, ActorId, typeof(TH).FullName, param);
        }

        /// <summary>取消延时</summary>
        public void RemoveTimer(long id)
        {
            QuartzTimer.Remove(id);
        }

        /// <summary>取消定时</summary>
        public void Unschedule(long id)
        {
            QuartzTimer.Remove(id);
        }
    }
}