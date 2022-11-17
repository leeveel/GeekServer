namespace Geek.Server
{
    public abstract class BaseRoleCompHandler : BaseCompHandler
    {
        protected override Task InitActor()
        {
            if (ActorId <= 0)
            {
                var session = SessionManager.GetByTargetId(TargetId);
                ActorId = session.Id;
            }
            return Task.CompletedTask;
        }
    }

    public abstract class RoleCompHandler<T> : BaseRoleCompHandler where T : ICompAgent
    {
        protected override Type CompAgentType => typeof(T);
        protected T Comp => (T)CacheComp;
    }

}
