
using Geek.Server.Proto;
using System.Collections.Concurrent;

namespace Geek.Server
{
    /// <summary>
    /// 管理玩家session，一个玩家一个，下线之后移除，顶号之后释放之前的channel，替换channel
    /// </summary>
    public static class SessionManager
    {
        internal static ConcurrentDictionary<long, Session> sessionMap = new();
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


        public static NetChannel GetChannel(long id)
        {
            sessionMap.TryGetValue(id, out Session session);
            return session?.GetNetChannel();
        }

        public static Session Get(long id)
        {
            sessionMap.TryGetValue(id, out Session session);
            return session;
        }

        public static Session GetByTargetId(long id)
        {
            foreach (var item in sessionMap)
            {
                if (item.Value.TargetId == id)
                {
                    return item.Value;
                }
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
                    //old.WriteAsync(msg);
                }
                else if (old.NodeId != session.NodeId)
                {
                    //do nothing
                    //同一个设备从不同的网络服重连上来
                }
            }
            sessionMap[session.Id] = session;
        }
    }
}
