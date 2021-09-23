using DotNetty.Common.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class SessionManager
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public static readonly AttributeKey<Session> SESSION = AttributeKey<Session>.ValueOf("SESSION");
        public static readonly ConcurrentDictionary<long, Session> sessionMap = new ConcurrentDictionary<long, Session>();
        public static readonly object lockObj = new object();

        public static Session Add(Session session)
        {
            Session old = null;
            lock (lockObj)
            {
                if (sessionMap.ContainsKey(session.Id))
                {
                    var oldChanel = sessionMap[session.Id];
                    if (oldChanel.Sign != session.Sign)
                    {
                        //顶号,老链接不断开，需要发送被顶号的消息
                        oldChanel.Channel.GetAttribute(SESSION).Set(null);
                        old = oldChanel;
                    }
                    else if (oldChanel.Channel != session.Channel)
                    {
                        oldChanel.Channel.CloseAsync();
                    }
                }
                session.Channel.GetAttribute(SESSION).Set(session);
                sessionMap[session.Id] = session;
            }
            return old;
        }

        public static Task Remove(long channelId)
        {
            sessionMap.TryRemove(channelId, out var channel);
            return Remove(channel);
        }

        public static async Task Remove(Session session)
        {
            if (session != null)
            {
                sessionMap.TryRemove(session.Id, out var se);
                if (se == null)
                    return;

                LOGGER.Info("移除channel {}", session.Id);
                var actor = await ActorManager.Get(session.Id);
                if (actor != null)
                    actor.EvtDispatcher.DispatchEvent((int)InnerEventID.OnDisconnected);
            }
        }

        public static Session Get(long sessionId)
        {
            sessionMap.TryGetValue(sessionId, out var session);
            return session;
        }

        public static async Task RemoveAll()
        {
            var taskList = new List<Task>();
            var list = sessionMap.Values;
            foreach (var ch in list)
            {
                _ = ch.Channel.CloseAsync();
                var actor = await ActorManager.Get(ch.Id);
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
