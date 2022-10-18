using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server
{
    public class TcpConnectionHandler : ConnectionHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public TcpConnectionHandler() { }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            OnConnection(connection);
            var channel = new NetChannel(connection, new LengthPrefixedProtocol());
            var remoteInfo = channel.Context.RemoteEndPoint;
            while (!channel.IsClose())
            {
                try
                {
                    var result = await channel.Reader.ReadAsync(channel.Protocol);
                    var message = result.Message;

                    if (result.IsCompleted)
                        break;

                    _ = Dispatcher(channel, MsgDecoder.Decode(connection, message));
                }
                catch (ConnectionResetException)
                {
                    LOGGER.Info($"{remoteInfo} ConnectionReset...");
                    break;
                }
                catch (ConnectionAbortedException)
                {
                    LOGGER.Info($"{remoteInfo} ConnectionAborted...");
                    break;
                }
                catch (Exception e)
                {
                    LOGGER.Error($"{remoteInfo} Exception:{e.Message}");
                }

                try
                {
                    channel.Reader.Advance();
                }
                catch (Exception e)
                {
                    LOGGER.Error($"{remoteInfo} channel.Reader.Advance Exception:{e.Message}");
                    break;
                }
            }
            OnDisconnection(channel);
        }

        protected void OnConnection(ConnectionContext connection)
        {
            LOGGER.Debug($"{connection.RemoteEndPoint?.ToString()} 链接成功");
        }

        protected void OnDisconnection(NetChannel channel)
        {
            LOGGER.Debug($"{channel.Context.RemoteEndPoint?.ToString()} 断开链接");
            var sessionId = channel.GetSessionId();
            if (sessionId > 0)
                HotfixMgr.SessionMgr.Remove(sessionId);
        }

        protected async Task Dispatcher(NetChannel channel, Message msg)
        {
            if (msg == null)
                return;

            //LOGGER.Debug($"-------------收到消息{msg.MsgId} {msg.GetType()}");
            var handler = TcpHandlerFactory.GetHandler(msg.MsgId);

            if (handler == null)
            {
                LOGGER.Error($"找不到[{msg.MsgId}][{msg.GetType()}]对应的handler");
                return;
            }

            if (handler is RoleTcpHandler actorHandler)
            {
                //被顶号，或者被关闭链接，不再处理消息
                long sessionId = channel.GetSessionId();
                if (sessionId == 0)
                {
                    return;
                }
                await actorHandler.InnerAction(channel, msg);
            }
            else
            {
                await handler.ActionAsync(channel, msg);
            }
        }
    }
}