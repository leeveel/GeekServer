using Geek.Server.App.Common.Handler;
using Geek.Server.App.Net.Session;
using Geek.Server.Core.Net.Tcp.Handler;

namespace Geek.Server.Hotfix.Login
{
    [MsgMapping(typeof(ReqLogin))]
    internal class ReqLoginHandler : GlobalCompHandler<LoginCompAgent>
    {
        public override async Task ActionAsync()
        {
            var session = new Session
            {
                NetId = ClientConnId,
                Channel = Channel
            };
            await Comp.OnLogin(session, Msg as ReqLogin);
        }
    }
}
