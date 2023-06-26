using Geek.Server.App.Common.Session;
using Geek.Server.Core.Net.BaseHandler;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Net.Websocket;

namespace Geek.Server.App.Common.Net
{
    public class AppWebSocketConnectionHandler : WebSocketConnectionHandler
    {
        protected override void OnDisconnection(INetChannel channel)
        {
            base.OnDisconnection(channel);
            var sessionId = channel.GetData<long>(SessionManager.SESSIONID);
            if (sessionId > 0)
                SessionManager.Remove(sessionId);
        }
    }
}
