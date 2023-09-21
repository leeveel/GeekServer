using Geek.Server.App.Common.Session;
using Geek.Server.Core.Net;
using Geek.Server.Core.Net.Tcp;

namespace Geek.Server.App.Common.Net
{
    public class AppTcpConnectionHandler : TcpConnectionHandler
    {
        protected override void OnDisconnection(NetChannel channel)
        {
            base.OnDisconnection(channel);
            var sessionId = channel.GetData<long>(SessionManager.SESSIONID);
            if (sessionId > 0)
                SessionManager.Remove(sessionId);
        }
    }
}
