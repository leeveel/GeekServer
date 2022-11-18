
namespace Geek.Server.Login
{
    [MsgMapping(typeof(ReqLogin))]
    internal class ReqLoginHandler : GlobalCompHandler<LoginCompAgent>
    {
        public override async Task ActionAsync()
        {
            var session = new Session
            {
                ClientConnId = ClientConnId,
                GateNodeId = GateNodeId
            };
            await Comp.OnLogin(session, Msg as ReqLogin);
        }
    }
}
