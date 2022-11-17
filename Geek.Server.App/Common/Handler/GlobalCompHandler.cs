namespace Geek.Server
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
