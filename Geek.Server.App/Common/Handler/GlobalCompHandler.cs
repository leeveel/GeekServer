using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Net.Tcp.Handler;
using Geek.Server.Core.Utils;

namespace Geek.Server.App.Common.Handler
{
    public abstract class BaseGlobalCompHandler : BaseCompHandler
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

    public abstract class GlobalCompHandler<T> : BaseGlobalCompHandler where T : ICompAgent
    {
        protected override Type CompAgentType => typeof(T);
        protected T Comp => (T)CacheComp;
    }

}
