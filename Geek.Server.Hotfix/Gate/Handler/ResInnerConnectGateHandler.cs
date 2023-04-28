using Geek.Server.App.Net;
using Geek.Server.Core.Net.Tcp.Handler;

namespace Geek.Server.Hotfix.Gate.Handler
{
    [MsgMapping(typeof(ResInnerConnectGate))]
    class ResInnerConnectGateHandler : BaseGatewayHandler
    {
        public override Task ActionAsync()
        {
            var res = Msg as ResInnerConnectGate;
            innerTcpClient.SyncNetProxies(res.ClientIds);
            return Task.CompletedTask;
        }
    }
}
