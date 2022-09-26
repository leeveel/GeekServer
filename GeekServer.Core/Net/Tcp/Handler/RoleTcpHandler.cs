
namespace Geek.Server
{
    public abstract class RoleTcpHandler : BaseTcpHandler
    {
        internal WeakReference<Actor> ActorRef;
        protected Actor Actor
        {
            get
            {
                if (ActorRef.TryGetTarget(out var actor))
                {
                    return actor;
                }
                return null;
            }
        }
        public virtual Task InnerAction()
        {
            Actor?.Tell(ActionAsync);
            return Task.CompletedTask;
        }
    }
}
