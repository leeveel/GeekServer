using System.Collections.Concurrent;
using Geek.Server.App.Common.Event;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Events;
using Geek.Server.Core.Net.BaseHandler;
using Geek.Server.Proto;
using NLog;

namespace Geek.Server.App.Common.Session
{
    /// <summary>
    /// 管理玩家session，一个玩家一个，下线之后移除，顶号之后释放之前的channel，替换channel
    /// </summary>
    public sealed class SessionManager
    {
        internal static readonly ConcurrentDictionary<long, Session> sessionMap = new();
        public const string SESSIONID = "SESSION_ID";
        public static int Count()
        {
            return sessionMap.Count;
        }

        public static void Remove(long id)
        { 
            if (sessionMap.TryRemove(id, out var _) && ActorMgr.HasActor(id))
            {
                EventDispatcher.Dispatch(id, (int)EventID.SessionRemove);
            }
        }

        public static Task RemoveAll()
        {
            foreach (var session in sessionMap.Values)
            {
                if (ActorMgr.HasActor(session.Id))
                {
                    EventDispatcher.Dispatch(session.Id, (int)EventID.SessionRemove);
                }
            }
            sessionMap.Clear();
            return Task.CompletedTask;
        }

        public static Session Get(long id)
        {
            sessionMap.TryGetValue(id, out Session session);
            return session;
        }

        public static void Add(Session session)
        {
            if (sessionMap.TryGetValue(session.Id, out var oldSession) && oldSession.Channel != session.Channel)
            {
                if (oldSession.Sign != session.Sign)
                {
                    var msg = new ResPrompt
                    {
                        Type = 5,
                        Content = "你的账号已在其他设备上登陆"
                    };
                    oldSession.WriteAsync(msg);
                }
                // 新连接 or 顶号
                oldSession.Channel.RemoveData(SESSIONID);
                oldSession.Channel.Close();
            }
            session.Channel.SetData(SESSIONID, session.Id);
            sessionMap[session.Id] = session;
        }
    }
}
