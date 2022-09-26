
using System.Diagnostics;
namespace Geek.Server.Login
{

    [MsgMapping(typeof(ReqLogin))]
    internal class ReqLoginHandler : BaseTcpHandler
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public override async Task ActionAsync()
        {
            var agent = await ActorMgr.GetCompAgent<LoginCompAgent>();
            var retMsg = await agent.OnLogin(Channel, Msg as ReqLogin);
            this.WriteWithErrCode(retMsg);
        }
    }
}
