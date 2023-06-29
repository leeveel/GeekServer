using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Net.BaseHandler;
using MessagePack;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Core.Net.Tcp
{
    public class TcpConnectionHandler : ConnectionHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public TcpConnectionHandler() { }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            OnConnection(connection);
            INetChannel channel = null;
            channel = new TcpChannel(connection, new DefaultMessageProtocol(), (msg) => _ = Dispatcher(channel, msg), () => OnDisconnection(channel));
            await channel.StartAsync();
        }

        protected virtual void OnConnection(ConnectionContext connection)
        {
            LOGGER.Debug($"{connection.RemoteEndPoint?.ToString()} 链接成功");
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