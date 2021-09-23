using System.Threading.Tasks;

namespace Geek.Server.Logic.Handler
{
    public abstract class RoleActorHandler : SingletonActorHandler
    {
        public override ActorType ActorType => ActorType.Role;
        public override Task<ComponentActor> GetActor()
        {
            var session = Channel.GetAttribute(SessionManager.SESSION).Get();
            return ActorManager.GetOrNew(session.Id);
        }
    }
}
