namespace Geek.Server
{
    public abstract class BaseComp
    {
        private ICompAgent _cacheAgent = null;
        private readonly object _cacheAgentLock = new();

        public ICompAgent GetAgent(Type refAssemblyType = null)
        {
            lock (_cacheAgentLock)
            {
                bool needActive = _cacheAgent == null;
                if (_cacheAgent != null && !HotfixMgr.DoingHotfix)
                    return _cacheAgent;
                var agent = HotfixMgr.GetAgent<ICompAgent>(this, refAssemblyType);
                _cacheAgent = agent;
                if (needActive)
                    agent.Active();
                return agent;
            }
        }

        public void ClearCacheAgent()
        {
            _cacheAgent = null;
        }

        internal Actor Actor { get; set; }

        internal long ActorId => Actor.Id;

        public virtual bool IsActive => true;

        public virtual Task Active()
        {
            return Task.CompletedTask;
        }

        public virtual async Task Deactive()
        {
            var agent = GetAgent();
            if (agent != null)
                await agent.Deactive();
        }

        internal virtual bool SaveState() { return true; }
    }
}
