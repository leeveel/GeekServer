using Geek.Server.App.Net.Session;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Net.Tcp.Handler;

namespace Geek.Server.App.Common.Handler
{
    public abstract class BaseRoleCompHandler : BaseCompHandler
    {
        public override async Task InitActor()
        {
            if (ActorId <= 0)
            {
                ActorId = Channel.GetData<long>(SessionManager.ROLE_ID);
            }
            await base.InitActor();
        }
    }

    public abstract class RoleCompHandler<T> : BaseRoleCompHandler where T : ICompAgent
    {
        protected override Type CompAgentType => typeof(T);
        protected T Comp => (T)CacheComp;
    }

}
