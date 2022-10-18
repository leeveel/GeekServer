
using System.Diagnostics;
namespace Geek.Server.Login
{

    [MsgMapping(typeof(ReqLogin))]
    internal class ReqLoginHandler : BaseTcpHandler
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public override async Task ActionAsync(NetChannel channel, Message msg)
        {
            var agent = await ActorMgr.GetCompAgent<LoginCompAgent>();
            var (state, retMsg) = await agent.OnLogin(channel, msg as ReqLogin);
            channel.WriteAsync(retMsg, msg.UniId, state);
        }
    }
}
