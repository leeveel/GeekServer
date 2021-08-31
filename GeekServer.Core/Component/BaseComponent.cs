using System;
using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class FuncComponent : BaseComponent
    {
    }

    public abstract class BaseComponent
    {
        protected IComponentAgent cacheAgent;
        public virtual IComponentAgent GetAgent(Type agentAssemblyType = null)
        {
            if (cacheAgent != null && !HotfixMgr.DoingHotfix)
                return cacheAgent;
            var agent = HotfixMgr.GetAgent<IComponentAgent>(this, agentAssemblyType);
            if(!HotfixMgr.DoingHotfix)
                cacheAgent = agent;
            return agent;
        }

        ///<summary>清理缓存agent</summary>
        public void ClearCacheAgent()
        {
            cacheAgent = null;
        }

        public TAgent GetAgentAs<TAgent>() where TAgent : IComponentAgent { return (TAgent)GetAgent(typeof(TAgent)); }

        public bool TransformAgent<T>(out T result) where T : class
        {
            var agent = GetAgent(typeof(T));
            result = agent as T;
            return result != null;
        }

        public Task<TComp> GetComponent<TComp>() where TComp : BaseComponent, new() { return Actor.GetComponent<TComp>(); }
        public ComponentActor Actor { get; protected set; }
        public bool IsActive { get; protected set; }
        public long ActorId => Actor.ActorId;
        public virtual Task Active()
        {
            IsActive = true;
            return Task.CompletedTask;
        }

        public virtual Task Deactive()
        {
            HotfixMgr.RemoveAgentCache(this);
            return Task.CompletedTask;
        }

        ///<summary>是否已经可以回收了</summary>
        internal virtual Task<bool> ReadyToDeactive()
        { 
            return Task.FromResult(true);
        }

        public virtual void Init(ComponentActor actor)
        {
            Actor = actor;
        }
    }
}