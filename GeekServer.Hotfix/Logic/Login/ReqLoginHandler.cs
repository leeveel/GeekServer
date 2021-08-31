using Geek.Server;
using Geek.Server.Logic.Handler;
using Geek.Server.Logic.Login;
using Geek.Server.Message.Login;
using System.Threading.Tasks;

namespace Logic.Login
{

    [TcpMsgMapping(typeof(ReqLogin))]
    public class ReqLoginHandler : SingletonActorHandler
    {
        public override ActorType ActorType => ActorType.Login;

        public override async Task ActionAsync()
        {
            var comp = await Actor.GetCompAgent<LoginCompAgent>();
            await comp.Login(Ctx, (ReqLogin)Msg);
        }
    }
}
