using Common.Net.Tcp;
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
            context.ConfigureAwait(false);
            LOGGER.Debug($"{context.RemoteEndPoint?.ToString()} 链接成功");
            NetChannel<NetMessage> channel = null;
            var id = IdGenerator.GetActorID(ActorType.Gate, Settings.ServerId);
            channel = new NetChannel<NetMessage>(context, new OuterProtocol(id), async (msg) => await Dispatcher(channel, msg));
            channel.NetId = id;
            GateNetMgr.AddClientNode(channel);
            await channel.StartReadMsgAsync();
            OnDisconnection(channel);
        }

        protected void OnDisconnection(INetChannel conn)
        {
            if (conn.NetId > 0)
            {
                GateNetMgr.RemoveClientNode(conn.NetId);
                var nmsg = new NetMessage(new ReqClientChannelInactive(), conn.NetId);
                var serverConn = GateNetMgr.GetServerNode(conn.DefaultTargetNodeId);
                serverConn?.Write(nmsg);
                conn.NetId = 0;
                LOGGER.Debug($"{conn.RemoteAddress} 断开链接");
            }
        }

        protected async ValueTask Dispatcher(INetChannel conn, NetMessage nmsg)
        {
            //LOGGER.Debug($"-------------收到消息{nmsg.MsgId}");
            var handler = MsgHanderFactory.GetHander(nmsg.MsgId);
            if (handler != null)
            {
                handler.Action(conn, nmsg.Deserialize());
                nmsg.ReturnRawMenory();
            }
            else
            {

                //LOGGER.Debug($"-------------分发消息{nmsg.MsgId}到{conn.DefaultTargetNodeId}");
                //分发到game server
                var serverConn = GateNetMgr.GetServerNode(conn.DefaultTargetNodeId);
                nmsg.NetId = conn.NetId;
                if (serverConn != null)
                    await serverConn.Write(nmsg);
            }
        }
    }
}