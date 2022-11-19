using Microsoft.AspNetCore.Connections;
using NLog;

namespace Geek.Server.Core.Net.Tcp
{
    public abstract class TcpConnectionHandler : ConnectionHandler
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            var conn = OnConnection(connection);
            var remoteInfo = conn.Channel.Context.RemoteEndPoint;
            while (!conn.Channel.IsClose())
            {
                try
                {
                    var result = await conn.Channel.Reader.ReadAsync(conn.Channel.Protocol);
                    var message = result.Message;

                    if (result.IsCompleted)
                        break;

                    Decode(conn, ref message);
                    Dispatcher(conn, message);
                }
                catch (ConnectionResetException)
                {
                    LOGGER.Info($"{remoteInfo} ConnectionReset...");
                    break;
                }
                catch (ConnectionAbortedException)
                {
                    LOGGER.Info($"{remoteInfo} ConnectionAborted...");
                    break;
                }
                catch (Exception e)
                {
                    LOGGER.Error($"{remoteInfo} Exception:{e.Message}");
                }

                try
                {
                    conn.Channel.Reader.Advance();
                }
                catch (Exception e)
                {
                    LOGGER.Error($"{remoteInfo} channel.Reader.Advance Exception:{e.Message}");
                    break;
                }
            }
            OnDisconnection(conn);
        }

        protected abstract Connection OnConnection(ConnectionContext context);

        protected abstract void OnDisconnection(Connection conn);

        protected abstract void Dispatcher(Connection conn, NetMessage nmsg);

        protected abstract void Decode(Connection conn, ref NetMessage nmsg);

    }
}