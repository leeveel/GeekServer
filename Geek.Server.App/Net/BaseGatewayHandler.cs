using Geek.Server.Core.Net.Tcp.Handler;

namespace Geek.Server.App.Net
{
    public abstract class BaseGatewayHandler : BaseTcpHandler
    {
        public InnerTcpClient innerTcpClient;
    }
}
