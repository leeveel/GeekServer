using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class SessionManager
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

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
                        oldChanel.Channel.RemoveSessionId();
                        old = oldChanel;
                    }
                    else if (!ReferenceEquals(oldChanel.Channel, session.Channel))
                    {
                        oldChanel.Channel.Abort();
                    }
                }
                session.Channel.SetSessionId(session.Id);
                sessionMap[session.Id] = session;
            }
            return old;
        }

        public static void Remove(long sessionId)
        {
            sessionMap.TryRemove(sessionId, out var channel);
            Remove(channel);
        }

        public static void Remove(Session session)
        {
            if (session != null)
            {
                sessionMap.TryRemove(session.Id, out var se);
                if (se == null)
                    return;

                LOGGER.Info("移除channel {}", session.Id);
                EventDispatcher.DispatchEvent(session.Id, (int)InnerEventID.OnDisconnected);
            }
        }

        public static Session Get(long sessionId)
        {
            sessionMap.TryGetValue(sessionId, out var session);
            return session;
        }

        public static long GetSessionId(NetChannel channel)
        {
            return channel.GetSessionId();
        }

        public static async Task RemoveAll()
        {
            var list = sessionMap.Values;
            foreach (var ch in list)
                ch.Channel.Abort();
            var task = EntityMgr.CompleteAllTask();
            //保证此函数执行完后所有actor队列为空
            if (await task.WaitAsyncCustom(TimeSpan.FromMinutes(10)))
                LOGGER.Error("remove all channel timeout");
        }
    }
}
