using System;


using DotNetty.Transport.Channels;

namespace Geek.Server
{
    public class TcpServerHandler : SimpleChannelInboundHandler<IMessage>
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        protected override void ChannelRead0(IChannelHandlerContext ctx, IMessage msg)
        {
            if (!Settings.Ins.AppRunning)
                return;

            //直接在当前io线程处理 
            IEventLoop group = ctx.Channel.EventLoop;
            group.Execute(async () =>
            {
                try
                {
                    var handler = TcpHandlerFactory.GetHandler(msg.GetMsgId());
                    LOGGER.Debug($"-------------get msg {msg.GetMsgId()} {msg.GetType()}");

                    if (handler == null)
                    {
                        LOGGER.Error("找不到对应的handler " + msg.GetMsgId());
                        return;
                    }

                    //握手
                    var session = ctx.Channel.GetAttribute(SessionManager.SESSION).Get();
                    if (session != null)
                        EventDispatcher.DispatchEvent(session.Id, (int)InnerEventID.OnMsgReceived);

                    handler.Time = DateTime.Now;
                    handler.Channel = ctx.Channel;
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
                                LOGGER.Error($"handler actor 为空 {msg.GetMsgId()} {handler.GetType()}");
                        }
                        else
                        {
                            LOGGER.Error($"EntityId 为0 {msg.GetMsgId()} {handler.GetType()} {session.Id}");
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
            });
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            LOGGER.Info("{} 连接成功.", context.Channel.RemoteAddress.ToString());
        }

        public override async void ChannelInactive(IChannelHandlerContext ctx)
        {
            LOGGER.Info("{} 断开连接.", ctx.Channel.RemoteAddress.ToString());
            var session = ctx.Channel.GetAttribute(SessionManager.SESSION).Get();
            try
            {
                await SessionManager.Remove(session);
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            LOGGER.Error(exception.ToString());
            context.CloseAsync();
        }
    }
}
