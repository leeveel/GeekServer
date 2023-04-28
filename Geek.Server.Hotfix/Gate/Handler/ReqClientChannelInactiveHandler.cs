using Geek.Server.App.Net;
using Geek.Server.Core.Net.Tcp.Handler;

namespace Geek.Server.Hotfix.Gate.Handler
{
    [MsgMapping(typeof(ReqClientChannelInactive))]
    class ReqClientChannelInactiveHandler : BaseGatewayHandler
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public override Task ActionAsync()
        {
            var proxy = innerTcpClient.GetProxy(ClientNetId);
            if (proxy != null)
            {
                proxy.Close();
                Log.Info($"客户端断开:{proxy.RemoteAddress}");
            }
            return Task.CompletedTask;
        }
    }
}
