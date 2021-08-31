using DotNetty.Transport.Channels;
using NLog;
using System;

namespace Geek.Server.Test
{
    public class RobotTcpHandler : SimpleChannelInboundHandler<IMessage>
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();
        protected override void ChannelRead0(IChannelHandlerContext ctx, IMessage msg)
        {
            if (!Settings.Ins.AppRunning)
                return;

            //直接在当前io线程处理 
            IEventLoop group = ctx.Channel.EventLoop;
            group.Execute(async () =>
            {
                var handler = TcpHandlerFactory.GetHandler(msg.GetMsgId());
                if (LogReceiveMsg(msg.GetMsgId()))
                    LOGGER.Debug($"-------------client msg {msg.GetMsgId()} {msg.GetType()}");

                if (handler != null)
                {
                    handler.Time = DateTime.Now; 
                    handler.Ctx = ctx;
                    handler.Msg = msg;
                    if (handler is TcpActorHandler actorHandler)
                    {
                        await actorHandler.GetActor();
                        if (actorHandler.Actor != null)
                            await actorHandler.Actor.SendAsync(actorHandler.ActionAsync);
                        else
                            LOGGER.Error($"handler actor 为空 {msg.GetMsgId()} {handler.GetType()}");
                        if (actorHandler is RobotHandler robotHandler)
                            _ = robotHandler.OnReciveMsg(0);
                    }
                    else
                    {
                        await handler.ActionAsync();
                    }
                }
                else
                {
                    LOGGER.Error("找不到对应的handler " + msg.GetMsgId());
                }
            });
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            LOGGER.Info("{} client连接成功.", context.Channel.RemoteAddress.ToString());
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            LOGGER.Info("{} client断开连接.", ctx.Channel.RemoteAddress.ToString());
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            //base.ExceptionCaught(context, exception);
            LOGGER.Error(exception.ToString());
            context.CloseAsync();
        }

        bool LogReceiveMsg(int msgId)
        {
            switch (msgId)
            {
                //心跳等消息不用打印
                default:
                    return true;
            }
        }
    }
}
