using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Net.Tcp.Codecs;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Core.Net.Tcp.Handler
{
    public class TcpConnectionHandler : ConnectionHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public TcpConnectionHandler() { }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            OnConnection(connection);
            NetChannel channel = null;
            channel = new NetChannel(connection, new LengthPrefixedProtocol(), (msg) => _ = Dispatcher(channel, msg), () => OnDisconnection(channel));
            await channel.StartReadMsgAsync();
        }

        protected virtual void OnConnection(ConnectionContext connection)
        {
            LOGGER.Debug($"{connection.RemoteEndPoint?.ToString()} 链接成功");
        }

        protected virtual void OnDisconnection(NetChannel channel)
        {
            LOGGER.Debug($"{channel.Context.RemoteEndPoint?.ToString()} 断开链接");
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