
using System.Collections.Concurrent;
using Geek.Server.Role;

namespace Geek.Server
{
    /// <summary>
    /// 管理玩家session，一个玩家一个，下线之后移除，顶号之后释放之前的channel，替换channel
    /// </summary>
    public sealed class SessionMgr : ISessionMgr
    {
        internal static readonly ConcurrentDictionary<long, Session> sessionMap = new();
        public static readonly object lockObj = new object();
        public async Task Remove(long id)
        {
            if (sessionMap.TryRemove(id, out var session) && ActorMgr.HasActor(id))
            {
                var roleComp = await ActorMgr.GetCompAgent<RoleCompAgent>(id);
                roleComp.Tell(roleComp.OnLogout);
            }
        }

        public async Task RemoveAll()
        {
            foreach (var session in sessionMap.Values)
            {
                if (ActorMgr.HasActor(session.Id))
                {
                    var roleComp = await ActorMgr.GetCompAgent<RoleCompAgent>(session.Id);
                    roleComp.Tell(roleComp.OnLogout);
                }
            }
            sessionMap.Clear();
        }

        public Session Get(long id)
        {
            sessionMap.TryGetValue(id, out Session session);
            return session;
        }

        public Session Add(Session session)
        {
            lock (lockObj)
            {
                Session old = null;
                if (sessionMap.ContainsKey(session.Id))
                {
                    var oldSession = sessionMap[session.Id];
                    if (oldSession.Sign != session.Sign)
                    {
                        //顶号,老链接不断开，需要发送被顶号的消息
                        oldSession.Id = 0;
                        old = oldSession;
                    }
                    else if (oldSession.netId != session.netId)
                    {
                        oldSession.Abort();
                    }
                }
                sessionMap[session.Id] = session;
                return old;
            }
        }
    }
}
