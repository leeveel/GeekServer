using Geek.Server.Gateway.MessageHandler;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Gateway.Logic.Net.Tcp.Outer
{
    public class OuterTcpConnectionHandler : TcpConnectionHandler
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        protected override Connection OnConnection(ConnectionContext context)
        {
            LOGGER.Debug($"{context.RemoteEndPoint?.ToString()} 链接成功");
            var conn = new Connection
            {
                Id = IdGenerator.GetActorID(ActorType.Role, Settings.ServerId),
                Channel = new NetChannel(context, new OuterProtocol())
            };
            GateNetMgr.ClientConns.Add(conn);
            return conn;
        }

        protected override void OnDisconnection(Connection conn)
        {
            LOGGER.Debug($"{conn.Channel.Context.RemoteEndPoint?.ToString()} 断开链接");
            GateNetMgr.ClientConns.Remove(conn);
        }

        protected override void Dispatcher(Connection conn, NMessage nmsg)
        {
            //LOGGER.Debug($"-------------收到消息{msg.MsgId} {msg.GetType()}");
            var handler = MsgHanderFactory.GetHander(nmsg.MsgId);
            if (handler != null)
            {
                handler.Action(conn, MessagePack.MessagePackSerializer.Deserialize<Message>(nmsg.MsgRaw));
            }
            else
            {
                //分发到game server
                var serverConn = GateNetMgr.ServerConns.GetByNodeId(conn.NodeId);
                nmsg.ClientConnId = conn.Id;
                serverConn?.WriteAsync(nmsg);
            }
        }

        protected override void Decode(Connection conn, ref NMessage nmsg)
        {
            OuterMsgDecoder.Decode(conn.Channel.Context, ref nmsg);
        }

    }
}