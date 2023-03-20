using Geek.Server.Core.Actors;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Net.Tcp.Inner;
using Geek.Server.Core.Utils;
using Geek.Server.Gateway.Net.Tcp.Handler;
using Microsoft.AspNetCore.Connections;
using System.Threading.Channels;

namespace Geek.Server.Gateway.Net.Tcp.Inner
{
    //服务器内部链接
    public class InnerTcpConnectionHandler : ConnectionHandler
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public override async Task OnConnectedAsync(ConnectionContext context)
        {
            LOGGER.Debug($"内部节点 {context.RemoteEndPoint?.ToString()} 链接成功");
            NetChannel channel = null;
            channel = new NetChannel(context, new InnerProtocol(true), (msg) => Dispatcher(channel, msg), () => OnDisconnection(channel));
            channel.Id = IdGenerator.GetActorID(ActorType.Role, Settings.ServerId);
            GateNetMgr.ServerConns.Add(channel);
            await channel.StartReadMsgAsync();
        }

        protected void OnDisconnection(NetChannel channel)
        {
            LOGGER.Debug($"{channel.Context.RemoteEndPoint?.ToString()} 断开链接");
            GateNetMgr.ServerConns.Remove(channel);
        }

        protected void Dispatcher(NetChannel conn, NetMessage nmsg)
        {
            var handler = MsgHanderFactory.GetHander(nmsg.MsgId);
            //如果是需要网关处理的消息
            if (handler != null)
            {
                handler.Action(conn, MessagePack.MessagePackSerializer.Deserialize<Message>(nmsg.MsgRaw));
            }
            else //否则转发
            {
                var clientConn = GateNetMgr.ClientConns.Get(nmsg.NetId);
                clientConn?.Write(nmsg);
            }
        }
    }
}