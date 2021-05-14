using DotNetty.Common.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class ChannelManager
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public static readonly AttributeKey<Channel> Att_Channel = AttributeKey<Channel>.ValueOf("Channel");
        public static readonly ConcurrentDictionary<long, Channel> channels = new ConcurrentDictionary<long, Channel>();
        public static readonly object lockObj = new object();

        public static Channel Add(Channel channel)
        {
            Channel old = null;
            lock (lockObj)
            {
                if (channels.ContainsKey(channel.Id))
                {
                    var oldChanel = channels[channel.Id];
                    if (oldChanel.Sign != channel.Sign)
                    {
                        //顶号,老链接不断开，需要发送被顶号的消息
                        oldChanel.Ctx.Channel.GetAttribute(Att_Channel).Set(null);
                        old = oldChanel;
                    }
                    else if (oldChanel.Ctx != channel.Ctx)
                    {
                        oldChanel.Ctx.CloseAsync();
                    }
                }
                channel.Ctx.Channel.GetAttribute(Att_Channel).Set(channel);
                channels[channel.Id] = channel;
            }
            return old;
        }

        public static Task Remove(long channelId)
        {
            channels.TryRemove(channelId, out var channel);
            return Remove(channel);
        }

        public static async Task Remove(Channel channel)
        {
            if (channel != null)
            {
                channels.TryRemove(channel.Id, out var se);
                if (se == null)
                    return;

                LOGGER.Info("移除channel {}", channel.Id);
                var actor = await ActorManager.Get<ComponentActor>(channel.Id);
                if (actor != null)
                {
                    if (actor is IChannel chl)
                        _ = actor.SendAsync(chl.OnDisconnect);
                    if (actor.TransformAgent<IChannel>(out var chAgent))
                        _ = actor.SendAsync(chAgent.OnDisconnect);
                }
            }
        }

        public static Channel Get(long channelId)
        {
            channels.TryGetValue(channelId, out var session);
            return session;
        }

        public static async Task RemoveAll()
        {
            var taskList = new List<Task>();
            var list = channels.Values;
            foreach (var ch in list)
            {
                _ = ch.Ctx.CloseAsync();
                var actor = await ActorManager.Get<ComponentActor>(ch.Id);
                if (actor != null)
                {
                    var task = actor.SendAsync(() => Task.Delay(1));
                    taskList.Add(task);
                }
                await Remove(ch);
            }
            //保证此函数执行完后所有actor队列为空
            if (await Task.WhenAll(taskList).WaitAsync(TimeSpan.FromSeconds(30)))
                LOGGER.Error("remove all channel timeout");
        }
    }
}
