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
using Geek.Core.Actor;
using DotNetty.Common.Utilities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Geek.Core.Hotfix;
using Geek.Core.Component;

namespace Geek.App.Session
{
    public class SessionManager
    {
        public static readonly AttributeKey<Session> SESSION = AttributeKey<Session>.ValueOf("SESSION");
        public static readonly ConcurrentDictionary<long, Session> sessions = new ConcurrentDictionary<long, Session>();
        public static readonly object lockObj = new object();

        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static Session Add(Session session)
        {
            Session toReplaceSession = null;
            lock (lockObj)
            {
                if (sessions.ContainsKey(session.Id))
                {
                    var oldSession = sessions[session.Id];
                    if (oldSession.Token != session.Token)
                    {
                        oldSession.Ctx.Channel.GetAttribute(SESSION).Set(null);
                        toReplaceSession = oldSession;
                    }
                    else if(oldSession.Ctx != session.Ctx)
                    {
                        oldSession.Ctx.CloseAsync();
                    }
                }
                session.Ctx.Channel.GetAttribute(SESSION).Set(session);
                sessions[session.Id] = session;
            }
            return toReplaceSession;
        }

        /// <summary>sessionId即roleId</summary>
        public static void Remove(long sessionId)
        {
            sessions.TryRemove(sessionId, out Session session);
            Remove(session);
        }

        public static async void Remove(Session session)
        {
            if (session != null)
            {
                sessions.TryRemove(session.Id, out var se);
                if (se == null)
                    return;

                LOGGER.Info("移除Session>{}", session.Id);
                var actor = await ActorManager.Get<ComponentActor>(session.Id);
                if (actor != null)
                {
                    if (actor is ISession ise)
                        ise.OnDisConnect();
                    if (actor.Agent is ISession seAgent)
                        seAgent.OnDisConnect();
                }
            }
        }

        public static async Task RemoveAll()
        {
            var taskList = new List<Task>();
            var list = sessions.Values;
            foreach (var se in list)
            {
                _ = se.Ctx.CloseAsync();
                var actor = await ActorManager.Get<ComponentActor>(se.Id);
                if(actor != null)
                {
                    var task = actor.SendAsync(() => Task.Delay(1));
                    taskList.Add(task);
                }
                Remove(se);
            }
            //保证此函数执行完后所有actor队列为空
            await Task.WhenAll(taskList);
        }

        /// <summary>sessionId即roleId/serverId</summary>
        public static Session Get(long sessionId)
        {
            sessions.TryGetValue(sessionId, out var session);
            return session;
        }
    }
}