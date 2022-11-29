using Geek.Server.Core.Actors;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Hotfix.Agent;

namespace Geek.Server.Core.Comps
{
    public abstract class BaseComp
    {
        private ICompAgent _cacheAgent = null;
        private readonly object _cacheAgentLock = new();

        public ICompAgent GetAgent(Type refAssemblyType = null)
        {
            lock (_cacheAgentLock)
            {
                if (_cacheAgent != null && !HotfixMgr.DoingHotfix)
                    return _cacheAgent;
                var agent = HotfixMgr.GetAgent<ICompAgent>(this, refAssemblyType);
                _cacheAgent = agent;
                return agent;
            }
        }

        public void ClearCacheAgent()
        {
            _cacheAgent = null;
        }

        internal Actor Actor { get; set; }

        internal long ActorId => Actor.Id;

        public bool IsActive { get; private set; } = false;

        public virtual Task Active()
        {
            IsActive = true;
            return Task.CompletedTask;
        }

        public virtual async Task Deactive()
        {
            var agent = GetAgent();
            if (agent != null)
                await agent.Deactive();
        }

        internal virtual Task SaveState() { return Task.CompletedTask; }

        internal virtual bool ReadyToDeactive => true;
    }
}
