using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Net.Tcp;
using System.Net.WebSockets;

namespace Geek.Server.Core.Net.Websocket
{
    public class WebSocketConnectionHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public virtual async Task OnConnectedAsync(WebSocket socket, string clientAddress)
        {
            LOGGER.Info($"new websocket {clientAddress} connect...");
            WebSocketChannel channel = null;
            channel = new WebSocketChannel(socket, clientAddress , (msg) => _ = Dispatcher(channel, msg));
            await channel.StartAsync();
            OnDisconnection(channel);
        }

        public virtual void OnDisconnection(NetChannel channel)
        {
            LOGGER.Debug($"{channel.RemoteAddress} 断开链接");
        }

        protected async Task Dispatcher(NetChannel channel, Message msg)
        {
            if (msg == null)
                return;

            //LOGGER.Debug($"-------------收到消息{msg.MsgId} {msg.GetType()}");
            var handler = HotfixMgr.GetTcpHandler(msg.MsgId);
            if (handler == null)
            {
                LOGGER.Error($"找不到[{msg.MsgId}][{msg.GetType()}]对应的handler");
                return;
            }
            handler.Msg = msg;
            handler.Channel = channel;
            await handler.Init();
            await handler.InnerAction();
        }
    }
}
