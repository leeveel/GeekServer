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
                var handler = TcpHandlerFactory.GetHandler(msg.GetMsgId());
                LOGGER.Debug($"-------------server msg {msg.GetMsgId()} {msg.GetType()}");

                if (handler == null)
                {
                    LOGGER.Error("找不到对应的handler " + msg.GetMsgId());
                    return;
                }

                //握手
                var channel = ctx.Channel.GetAttribute(ChannelManager.Att_Channel).Get();
                if (channel != null)
                {
                    var actor = await ActorManager.Get<ComponentActor>(channel.Id);
                    if (actor != null)
                    {
                        if (actor is IChannel ise)
                            _ = actor.SendAsync(ise.Hand);
                        if (actor.TransformAgent<IChannel>(out var seAgent))
                            _ = actor.SendAsync(seAgent.Hand);
                    }
                }

                handler.Time = DateTime.Now;
                handler.Ctx = ctx;
                handler.Msg = msg;
                if (handler is TcpActorHandler actorHandler)
                {
                    actorHandler.Actor = await actorHandler.CacheActor();
                    if (actorHandler.Actor != null)
                        await actorHandler.Actor.SendAsync(actorHandler.ActionAsync);
                    else
                        LOGGER.Error($"handler actor 为空 {msg.GetMsgId()} {handler.GetType()}");
                }
                else
                {
                    await handler.ActionAsync();
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
            var channel = ctx.Channel.GetAttribute(ChannelManager.Att_Channel).Get();
            await ChannelManager.Remove(channel);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            LOGGER.Error(exception.ToString());
            context.CloseAsync();
        }
    }
}
