
namespace Geek.Server
{

    public abstract class RoleCompHandler : BaseTcpHandler
    {
        protected long ActorId { get; set; }

        protected abstract Type CompAgentType { get; }

        public ICompAgent CacheComp { get; set; }

        protected virtual Task InitActor()
        {
            if (ActorId <= 0)
                ActorId = Channel.GetSessionId();
            return Task.CompletedTask;
        }

        public override async Task Init()
        {
            await InitActor();
            if (CacheComp == null)
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

    public abstract class RoleCompHandler<T> : RoleCompHandler where T : ICompAgent
    {
        protected override Type CompAgentType => typeof(T);

        protected T Comp => (T)CacheComp;
    }


}
