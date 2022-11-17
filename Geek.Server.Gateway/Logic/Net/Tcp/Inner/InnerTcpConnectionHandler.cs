using Geek.Server.Gateway.MessageHandler;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Gateway.Logic.Net
{
    public class InnerTcpConnectionHandler : ConnectionHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public InnerTcpConnectionHandler() { }

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

                    InnerMsgDecoder.Decode(ref message);
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
                //Id = IdGenerator.GetActorID(ActorType.Role, Settings.ServerId),
                Channel = new NetChannel(context, new InnerProtocol())
            };
            //GateNetHelper.ServerConns.Add(conn);
            return conn;
        }

        protected void OnDisconnection(Connection conn)
        {
            LOGGER.Debug($"{conn.Channel.Context.RemoteEndPoint?.ToString()} 断开链接");
            GateNetHelper.ServerConns.Remove(conn);
        }


        protected void Dispatcher(Connection conn, NMessage nmsg)
        {
            //LOGGER.Debug($"-------------收到消息{msg.MsgId} {msg.GetType()}");
            var handler = MsgHanderFactory.GetHander(nmsg.MsgId);
            if (handler != null)
            {
                handler.Action(conn, MessagePack.MessagePackSerializer.Deserialize<Message>(nmsg.MsgRaw));
            }
            else
            {
                //分发到客户端(客户端有可能已经断开，找不到)
                var clientConn = GateNetHelper.ClientConns.Get(nmsg.TargetId); 
                clientConn?.Channel.WriteAsync(nmsg);
            }
        }

    }
}