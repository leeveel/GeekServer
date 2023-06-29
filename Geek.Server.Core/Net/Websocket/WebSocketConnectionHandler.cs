using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Net.BaseHandler;
using Geek.Server.Core.Net.Tcp;
using System.Net.WebSockets;

namespace Geek.Server.Core.Net.Websocket
{
    public class WebSocketConnectionHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public virtual Task OnConnectedAsync(WebSocket socket)
        {
            LOGGER.Info("new websocket connect...");
            WebSocketChannel channel = null;
            channel = new WebSocketChannel(socket, new DefaultMessageProtocol(), (msg) => _ = Dispatcher(channel, msg), () => OnDisconnection(channel));
            return channel.StartAsync();
        }

        protected virtual void OnDisconnection(INetChannel channel)
        {
            LOGGER.Debug($"{channel.RemoteAddress} 断开链接");
        }

        protected async Task Dispatcher(INetChannel channel, Message msg)
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
