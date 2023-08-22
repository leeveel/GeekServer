using Geek.Server.App.Common.Event;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Events;
using Geek.Server.Proto;
using NLog;
using System.Collections.Concurrent;

namespace Geek.Server.App.Net.Session
{
    /// <summary>
    /// 管理玩家session，一个玩家一个，下线之后移除，顶号之后释放之前的channel，替换channel
    /// </summary>
    public static class SessionManager
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static readonly string SESSION = "SESSION";
        public static readonly string ROLE_ID = "R_ID";
        internal static ConcurrentDictionary<long, GameSession> sessionMap = new();

        public static int Count()
        {
            return sessionMap.Count;
        }

        public static void Remove(GameSession session)
        {
            var id = session.RoleId;
            lock(sessionMap)
            {
                if (sessionMap.TryGetValue(id, out GameSession curSession) && session == curSession)
                {
                    sessionMap.TryRemove(id, out _);
                    Log.Info($"移除session:{id} ,channel Id:{session.Channel?.NetId}");
                    session.Channel.SetData(SESSION, null);
                    if (ActorMgr.HasActor(id))
                        EventDispatcher.Dispatch(id, (int)EventID.SessionRemove);
                }
            } 
        } 

        public static Task RemoveAll()
        {
            foreach (var session in sessionMap.Values)
            {
                if (ActorMgr.HasActor(session.RoleId))
                {
                    EventDispatcher.Dispatch(session.RoleId, (int)EventID.SessionRemove);
                }
            }
            sessionMap.Clear();
            return Task.CompletedTask;
        }

        public static GameSession Get(long id)
        {
            sessionMap.TryGetValue(id, out GameSession session);
            return session;
        }

        public static void Add(GameSession session)
        {
            if (sessionMap.TryGetValue(session.RoleId, out GameSession old))
            {
                if (old.Sign != session.Sign)
                {
                    Log.Debug("old.Token != session.Token，被顶号");
                    //顶号
                    var msg = new ResPrompt
                    {
                        Type = 5,
                        Content = "你的账号已在其他设备上登陆"
                    };
                    old.Write(msg);
                }
                else if (old.Channel != session.Channel)
                {
                    Log.Debug("old.Channel != session.Channel，断开老连接");
                    //do nothing
                    //同一个设备从不同的网络服重连上来 
                }
                old.Channel.Close();
            }

            session.Channel.SetData(SESSION, session);
            session.Channel.SetData(ROLE_ID, session.RoleId);
            sessionMap[session.RoleId] = session;
        }
    }
}
