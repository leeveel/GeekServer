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
            while (true)
            {
                try
                {
                    var result = await channel.Reader.ReadAsync(channel.Protocol);
                    var message = result.Message;

                    if (result.IsCompleted)
                        break;

                    //LOGGER.Debug("Received a message of {Length} bytes", message.Payload.Length);
                    //分发消息
                    _ = Dispatcher(channel, MsgDecoder.Decode(connection, message));
                }
                catch (ConnectionResetException)
                {
                    break;
                }
                catch (ConnectionAbortedException)
                {
                    break;
                }
                finally
                {
                    channel.Reader.Advance();
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
                SessionManager.Remove(sessionId);
        }

        protected async Task Dispatcher(NetChannel channel, Message msg)
        {
            if (msg == null)
                return;
            try
            {
                var handler = TcpHandlerFactory.GetHandler(msg.MsgId);
                LOGGER.Debug($"-------------get msg {msg.MsgId} {msg.GetType()}");

                if (handler == null)
                {
                    LOGGER.Error("找不到对应的handler " + msg.MsgId);
                    return;
                }

                //握手
                long sessionId = channel.GetSessionId();
                if (sessionId > 0)
                    EventDispatcher.DispatchEvent(sessionId, (int)InnerEventID.OnMsgReceived);

                handler.Time = DateTime.Now;
                handler.Channel = channel;
                handler.Msg = msg;
                if (handler is TcpCompHandler compHandler)
                {
                    var entityId = await compHandler.GetEntityId();
                    if (entityId != 0)
                    {
                        var agent = await EntityMgr.GetCompAgent(entityId, compHandler.CompAgentType);
                        if (agent != null)
                            _ = agent.Owner.Actor.SendAsync(compHandler.ActionAsync);
                        else
                            LOGGER.Error($"handler actor 为空 {msg.MsgId} {handler.GetType()}");
                    }
                    else
                    {
                        LOGGER.Error($"EntityId 为0 {msg.MsgId} {handler.GetType()} {sessionId}");
                    }
                }
                else
                {
                    await handler.ActionAsync();
                }
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
        }


    }
}