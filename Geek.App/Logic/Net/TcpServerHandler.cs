/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using NLog;
using System;
using Geek.Core.Net.Message;
using DotNetty.Transport.Channels;
using Geek.Core.Net.Handler;
using Geek.App.Session;
using Geek.Core.Actor;
using Geek.Core.Hotfix;
using Geek.Core.Component;

namespace Geek.App.Net
{
    public class TcpServerHandler : SimpleChannelInboundHandler<IMessage>
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
                    LOGGER.Debug($"-------------server msg {msg.GetMsgId()} {msg.GetType()}");

                if (handler == null)
                {
                    LOGGER.Error("找不到对应的handler " + msg.GetMsgId());
                    return;
                }

                //握手
                var session = ctx.Channel.GetAttribute(SessionManager.SESSION).Get();
                if (session != null)
                {
                    var actor = await ActorManager.Get<ComponentActor>(session.Id);
                    if (actor != null)
                    {
                        if (actor is ISession ise)
                            ise.Hand();
                        if (actor.Agent is ISession seAgent)
                            seAgent.Hand();
                    }
                }

                handler.Time = TimeUtils.CurrentTimeMillis();
                handler.Ctx = ctx;
                handler.Msg = msg;
                if (handler is TcpActorHandler actorHandler)
                {
                    actorHandler.CacheActor();
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

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            LOGGER.Info("{} 断开连接.", ctx.Channel.RemoteAddress.ToString());
            var session = ctx.Channel.GetAttribute(SessionManager.SESSION).Get();
            SessionManager.Remove(session);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            //base.ExceptionCaught(context, exception);
            LOGGER.Error(exception.ToString());
            context.CloseAsync();
        }

        bool LogReceiveMsg(int msgId)
        {
            switch(msgId)
            {
                //心跳等消息不用打印
                default:
                    return true;
            }
        }
    }
}
