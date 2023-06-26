using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Net.Tcp.Handler;
using Geek.Server.Core.Utils;

namespace Geek.Server.Core.Net.BaseHandler
{
    public abstract class BaseCompHandler : BaseMessageHandler
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

    public abstract class RoleCompHandler : BaseCompHandler
    {
        protected override Task InitActor()
        {
            if (ActorId <= 0)
                ActorId = Channel.GetData<long>("SESSION_ID");
            return Task.CompletedTask;
        }
    }


    public abstract class GlobalCompHandler : BaseCompHandler
    {
        protected override Task InitActor()
        {
            if (ActorId <= 0)
            {
                var compType = CompAgentType.BaseType.GetGenericArguments()[0];
                ActorType actorType = CompRegister.GetActorType(compType);
                ActorId = IdGenerator.GetActorID(actorType);
            }
            return Task.CompletedTask;
        }
    }

    public abstract class RoleCompHandler<T> : RoleCompHandler where T : ICompAgent
    {
        protected override Type CompAgentType => typeof(T);
        protected T Comp => (T)CacheComp;
    }

    public abstract class GlobalCompHandler<T> : GlobalCompHandler where T : ICompAgent
    {
        protected override Type CompAgentType => typeof(T);
        protected T Comp => (T)CacheComp;
    }

}
