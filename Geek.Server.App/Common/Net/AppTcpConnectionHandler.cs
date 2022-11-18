namespace Geek.Server
{
    public class AppTcpConnectionHandler : TcpConnectionHandler
    {
        protected override void OnDisconnection(NetChannel channel)
        {
            base.OnDisconnection(channel);
            var sessionId = channel.GetSessionId();
            if (sessionId > 0)
                SessionManager.Remove(sessionId);
        }
    }
}
