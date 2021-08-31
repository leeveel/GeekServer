using System.Threading.Tasks;

namespace Geek.Server.Logic.Handler
{
    public abstract class RoleActorHandler : TcpActorHandler
    {

        public override Task<ComponentActor> GetActor()
        {
            var session = Ctx.GetAttribute(SessionManager.SESSION).Get();
            return ActorManager.GetOrNew(session.Id);
        }

    }
}
