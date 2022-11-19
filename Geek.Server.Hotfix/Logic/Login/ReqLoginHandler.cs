
using Geek.Server.Core.Net.Tcp.Handler;
using Geek.Server.Proto;

namespace Server.Logic.Logic.Login
{
    [MsgMapping(typeof(ReqLogin))]
    internal class ReqLoginHandler : GlobalCompHandler<LoginCompAgent>
    {
        public override async Task ActionAsync()
        {
            await Comp.OnLogin(Channel, Msg as ReqLogin);
        }
    }
}
