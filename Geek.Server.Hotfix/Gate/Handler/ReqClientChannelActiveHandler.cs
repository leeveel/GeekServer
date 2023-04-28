using Geek.Server.App.Net;
using Geek.Server.Core.Net.Tcp.Handler;

namespace Geek.Server.Hotfix.Gate.Handler
{
    [MsgMapping(typeof(ReqClientChannelActive))]
    class ReqClientChannelActiveHandler : BaseGatewayHandler
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public override Task ActionAsync()
        {
            var req = (ReqClientChannelActive)Msg;
            innerTcpClient.AddProxy(req.NetId, new NetChannelProxy(innerTcpClient, req.NetId, req.Address));
            Log.Info($"客户端连接:{req.Address}");
            return Task.CompletedTask;
        }
    }
}
