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
using Base;
using Geek.Core.Events;
using Geek.Hotfix.Logic.Role;
using Geek.App.Role;
using System.Threading.Tasks;

namespace Geek.Hotfix.Events
{
    public static class EventExt
    {
        public static void AddListener<T>(this EventDispatcher dispatcher, EventID evtId) where T : IEventHandler
        {
            dispatcher.AddListener((int)evtId, typeof(T).FullName);
        }

        public static void RemoveListener<T>(this EventDispatcher dispatcher, EventID evtId) where T : IEventHandler
        {
            dispatcher.RemoveListener((int)evtId, typeof(T).FullName);
        }

        public static void DispatchEvent(this EventDispatcher dispatcher, EventID evtId, Param param = null)
        {
            dispatcher.DispatchEvent((int)evtId, param);
        }


        public static void AddListener<T>(this CrossActorEventDispatcher<RoleActor> dispatcher, long roleId, EventID evtId) where T : IEventHandler
        {
            dispatcher.AddListener(roleId, (int)evtId, typeof(T).FullName);
        }

        public static void RemoveListener<T>(this CrossActorEventDispatcher<RoleActor> dispatcher, long roleId, EventID evtId) where T : IEventHandler
        {
            dispatcher.RemoveListener(roleId, (int)evtId, typeof(T).FullName);
        }

        public static void ClearListener(this CrossActorEventDispatcher<RoleActor> dispatcher, long roleId)
        {
            dispatcher.ClearListener(roleId);
        }

        public static void DispatchEvent(this CrossActorEventDispatcher<RoleActor> dispatcher, EventID evtId, Param param = null)
        {
            dispatcher.DispatchEvent((int)evtId, checkRoleDispatch, param);
        }

        static async Task<bool> checkRoleDispatch(RoleActor actor)
        {
            if (actor == null)
                return false;
            if (!(await actor.GetAgentAs<RoleActorAgent>().IsOnline()))
                return false;
            return true;
        }
    }
}
