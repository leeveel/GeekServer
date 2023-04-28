using Geek.Server.Core.Actors;
using Geek.Server.Core.Hotfix.Agent;

namespace Geek.Server.Core.Net.Tcp.Handler
{

    public abstract class BaseCompHandler : BaseTcpHandler
    {
        protected long ActorId { get; set; }

        protected abstract Type CompAgentType { get; }

        public ICompAgent CacheComp { get; set; }

        public virtual async Task InitActor()
        {
            if (CacheComp == null && ActorId > 0)
                CacheComp = await ActorMgr.GetCompAgent(ActorId, CompAgentType);
        }

        public override Task InnerAction()
        {
            CacheComp.Tell(ActionAsync);
            return Task.CompletedTask;
        }

        protected Task<OtherAgent> GetCompAgent<OtherAgent>() where OtherAgent : ICompAgent
        {
            return CacheComp.GetCompAgent<OtherAgent>();
        }
    }

}
