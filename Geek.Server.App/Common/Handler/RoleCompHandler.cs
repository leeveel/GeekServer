using Geek.Server.App.Net.Session;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Net.Tcp.Handler;

namespace Geek.Server.App.Common.Handler
{
    public abstract class BaseRoleCompHandler : BaseCompHandler
    {
        protected override Task InitActor()
        {
            if (ActorId <= 0)
            {
                var session = SessionManager.GetByClientConnId(ClientConnId);
                ActorId = session.RoleId;
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
