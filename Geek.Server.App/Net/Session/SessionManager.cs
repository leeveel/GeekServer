using Geek.Server.App.Common.Event;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Events;
using Geek.Server.Proto;
using System.Collections.Concurrent;

namespace Geek.Server.App.Net.Session
{
    /// <summary>
    /// 管理玩家session，一个玩家一个，下线之后移除，顶号之后释放之前的channel，替换channel
    /// </summary>
    public static class SessionManager
    {
        internal static ConcurrentDictionary<long, Session> sessionMap = new();
        //connId - session.id
        internal static readonly ConcurrentDictionary<long, long> connIdMap = new();

        public static int Count()
        {
            return sessionMap.Count;
        }

        public static void Remove(long id)
        {
            if (sessionMap.TryRemove(id, out Session session))
            {
                connIdMap.TryRemove(session.ClientConnId, out _);
                if (ActorMgr.HasActor(id))
                    EventDispatcher.Dispatch(id, (int)EventID.SessionRemove);
            }
        }

        public static void RemoveByClientConnId(long id)
        {
            var session = GetByClientConnId(id);
            if(session != null)
                Remove(session.Id);
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
            connIdMap.Clear();
            return Task.CompletedTask;
        }

        public static Session Get(long id)
        {
            sessionMap.TryGetValue(id, out Session session);
            return session;
        }

        public static Session GetByClientConnId(long connId)
        {
            if (connIdMap.TryGetValue(connId, out long id))
            {
                sessionMap.TryGetValue(id, out Session session);
                return session;
            }
            return null;
        }

        public static void Add(Session session)
        {
            if (sessionMap.TryGetValue(session.Id, out Session old))
            {
                if (old.Token != session.Token)
                {
                    //顶号
                    var msg = new ResPrompt
                    {
                        Type = 5,
                        Content = "你的账号已在其他设备上登陆"
                    };
                    old.WriteAsync(msg);
                }
                else if (old.GateNodeId != session.GateNodeId)
                {
                    //do nothing
                    //同一个设备从不同的网络服重连上来
                }
            }
            sessionMap[session.Id] = session;
            connIdMap[session.ClientConnId] = session.Id;
        }
    }
}
