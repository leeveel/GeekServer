
namespace Geek.Server
{

    public abstract class BaseCompHandler : BaseTcpHandler
    {
        protected long ActorId { get; set; }

        protected abstract Type CompAgentType { get; }

        public ICompAgent CacheComp { get; set; }

        protected abstract Task InitActor();

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

}
