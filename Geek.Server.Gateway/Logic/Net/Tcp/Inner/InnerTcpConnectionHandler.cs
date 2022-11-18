using Geek.Server.Gateway.MessageHandler;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Gateway.Logic.Net
{
    public class InnerTcpConnectionHandler : TcpConnectionHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        protected override Connection OnConnection(ConnectionContext context)
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

        protected override void OnDisconnection(Connection conn)
        {
            LOGGER.Debug($"{conn.Channel.Context.RemoteEndPoint?.ToString()} 断开链接");
            GateNetMgr.ServerConns.Remove(conn);
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
                //分发到客户端(客户端有可能已经断开，找不到)
                var clientConn = GateNetMgr.ClientConns.Get(nmsg.TargetId); 
                clientConn?.Channel.WriteAsync(nmsg);
            }
        }

        protected override void Decode(Connection conn, ref NMessage nmsg)
        {
            InnerMsgDecoder.Decode(ref nmsg);
        }

    }
}