using Geek.Server.Core.Actors;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Utils;
using Geek.Server.Gateway.Net.Tcp.Handler;
using Geek.Server.Proto;
using Microsoft.AspNetCore.Connections;
using MongoDB.Driver.Core.Bindings;
using System.Threading.Channels;

namespace Geek.Server.Gateway.Net.Tcp.Outer
{
    //与客户端链接处理
    public class OuterTcpConnectionHandler : ConnectionHandler
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public override async Task OnConnectedAsync(ConnectionContext context)
        {
            LOGGER.Debug($"{context.RemoteEndPoint?.ToString()} 链接成功");
            NetChannel channel = null;
            channel = new NetChannel(context, new OuterProtocol(), (msg) => Dispatcher(channel, msg), () => OnDisconnection(channel));
            channel.Id = IdGenerator.GetActorID(ActorType.Role, Settings.ServerId);
            GateNetMgr.ClientConns.Add(channel);
            await channel.StartReadMsgAsync();
        }

        protected void OnDisconnection(NetChannel conn)
        {
            LOGGER.Debug($"{conn.Context.RemoteEndPoint?.ToString()} 断开链接");
            GateNetMgr.ClientConns.Remove(conn);
            //TODO:通知游戏服客户端掉线
            var msg = new PlayerDisconnected
            {
                GateNodeId = Settings.ServerId
            };
            var nmsg = new NetMessage
            {
                NetId = conn.Id,
                Msg = msg,
                MsgId = msg.MsgId
            };
            var serverConn = GateNetMgr.ServerConns.GetByNodeId(conn.NodeId);
            serverConn?.Write(nmsg);
        }

        protected void Dispatcher(NetChannel conn, NetMessage nmsg)
        {
            LOGGER.Debug($"-------------收到消息{nmsg.MsgId}");
            var handler = MsgHanderFactory.GetHander(nmsg.MsgId);
            if (handler != null)
            {
                handler.Action(conn, MessagePack.MessagePackSerializer.Deserialize<Message>(nmsg.MsgRaw));
            }
            else
            {
                //分发到game server
                var serverConn = GateNetMgr.ServerConns.GetByNodeId(conn.NodeId);
                nmsg.NetId = conn.Id;
                serverConn?.Write(nmsg);
            }
        }
    }
}