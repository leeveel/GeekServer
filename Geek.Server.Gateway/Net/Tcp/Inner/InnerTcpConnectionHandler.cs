using Common.Net.Tcp;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Gateway.Net.Tcp.Handler;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Gateway.Net.Tcp.Inner
{
    //服务器内部链接
    public class InnerTcpConnectionHandler : ConnectionHandler
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public override async Task OnConnectedAsync(ConnectionContext context)
        {
            LOGGER.Debug($"内部节点 {context.RemoteEndPoint?.ToString()} 链接成功");
            NetChannel<NetMessage> channel = null;
            channel = new NetChannel<NetMessage>(context, new InnerProtocol(), async (msg) => await Dispatcher(channel, msg));
            await channel.StartReadMsgAsync();
            OnDisconnection(channel);
        }

        protected void OnDisconnection(INetChannel channel)
        {
            LOGGER.Debug($"{channel.RemoteAddress} 断开链接");
            GateNetMgr.RemoveServerNode(channel.NetId);
        }

        protected async ValueTask Dispatcher(INetChannel conn, NetMessage nmsg)
        {
            var handler = MsgHanderFactory.GetHander(nmsg.MsgId);
            //如果是需要网关处理的消息
            if (handler != null)
            {
                handler.Action(conn, MessagePack.MessagePackSerializer.Deserialize<Message>(nmsg.MsgRaw));
            }
            else //否则转发
            {
                //LOGGER.Debug($"转发消息:{nmsg.MsgId}到{nmsg.SrcNetId}");
                var clientConn = GateNetMgr.GetClientNode(nmsg.NetId);
                if (clientConn != null)
                    await clientConn.Write(nmsg);
            }
            nmsg.ReturnRawMenory();
        }
    }
}