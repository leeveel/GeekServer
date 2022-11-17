using Geek.Server.Gateway.MessageHandler;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Gateway.Logic.Net.Tcp.Outer
{
    public class OuterTcpConnectionHandler : ConnectionHandler
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public OuterTcpConnectionHandler() { }

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

                    OuterMsgDecoder.Decode(connection, ref message);
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

        protected Connection OnConnection(ConnectionContext context)
        {
            LOGGER.Debug($"{context.RemoteEndPoint?.ToString()} 链接成功");
            var conn = new Connection
            {
                Id = IdGenerator.GetActorID(ActorType.Role, Settings.ServerId),
                Channel = new NetChannel(context, new OuterProtocol())
            };
            GateNetHelper.ClientConns.Add(conn);
            return conn;
        }

        protected void OnDisconnection(Connection conn)
        {
            LOGGER.Debug($"{conn.Channel.Context.RemoteEndPoint?.ToString()} 断开链接");
            GateNetHelper.ClientConns.Remove(conn);
        }

        protected void Dispatcher(Connection conn, NMessage msg)
        {
            //LOGGER.Debug($"-------------收到消息{msg.MsgId} {msg.GetType()}");
            var handler = MsgHanderFactory.GetHander(msg.MsgId);
            if (handler != null)
            {
                handler.Action(conn, MessagePack.MessagePackSerializer.Deserialize<Message>(msg.MsgRaw));
            }
            else
            {
                //分发到game server
                var serverConn = GateNetHelper.ServerConns.Get(conn.TargetId);
                msg.TargetId = conn.Id;
                serverConn.Channel.WriteAsync(msg);
            }
        }
    }
}