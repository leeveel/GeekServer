using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class ClientNetChannel : NetChannel, IThreadPoolWorkItem
    {

        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public ClientNetChannel(ConnectionContext context, IProtocal<NMessage> protocal)
            : base(context, protocal)
        {
            ThreadPool.UnsafeQueueUserWorkItem(this, true);
            //Task.Run(NetLooping);
        }

        public void Execute()
        {
            //throw new NotImplementedException();
            _ = NetLooping();
        }

        protected override void ConnectionClosed()
        {
            base.ConnectionClosed();
            LOGGER.Debug($"{Context.RemoteEndPoint?.ToString()} 服务器断开链接");
            connectionClosed = true;
        }

        private bool connectionClosed = false;
        private async Task NetLooping()
        {
            while (!connectionClosed)
            {
                try
                {
                    var result = await Reader.ReadAsync(Protocol);

                    var message = result.Message;
                    //分发消息
                    _ = Dispatcher(Decode(message));

                    if (result.IsCompleted)
                        break;
                }
                catch (ConnectionResetException)
                {
                    LOGGER.Debug("{ConnectionId} disconnected", Context.ConnectionId);
                    break;
                }
                finally
                {
                    Reader.Advance();
                }
            }
        }

        private IMessage Decode(NMessage message)
        {
            var reader = new SequenceReader<byte>(message.Payload);

            //数据包长度消息头两个int 8位
            if (message.Payload.Length < MSG_HEAD_LENGTH)
                return null;

            //消息id
            reader.TryReadBigEndian(out int msgId);

            var msg = TcpHandlerFactory.GetMsg(msgId);
            if (msg == null)
            {
                LOGGER.Error("消息ID:{} 找不到对应的Msg.", msgId);
                return null;
            }

            if (msg.MsgId == msgId)
            {
                var span = new Span<byte>(new byte[reader.Length - 4]);
                reader.TryCopyTo(span);
                msg.Deserialize(span.ToArray());
                return msg;
            }
            else
            {
                LOGGER.Error("后台解析消息失败，注册消息id和消息无法对应.real:{0}, register:{1}", msg.MsgId, msgId);
                return null;
            }
        }


        const int MSG_HEAD_LENGTH = sizeof(int) * 2;
        public async Task Dispatcher(IMessage msg)
        {
            try
            {
                if (msg == null)
                    return;

                var handler = TcpHandlerFactory.GetHandler(msg.MsgId);
                LOGGER.Debug($"-------------get msg {msg.MsgId} {msg.GetType()}");

                if (handler == null)
                {
                    LOGGER.Error("找不到对应的handler " + msg.MsgId);
                    return;
                }

                long sessionId = GetSessionId();
                if (sessionId > 0)
                    EventDispatcher.DispatchEvent(sessionId, (int)InnerEventID.OnMsgReceived);

                handler.Time = DateTime.Now;
                handler.Channel = this;
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
                LOGGER.Error(e, "解析数据异常," + e.Message + "\n" + e.StackTrace);
            }
        }

       
    }
}
