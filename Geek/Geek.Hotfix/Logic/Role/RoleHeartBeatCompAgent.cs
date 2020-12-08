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
using System;
using Geek.App.Role;
using Geek.Core.Hotfix;
using System.Threading.Tasks;
using Geek.Core.Timer;
using Geek.App.Session;
using Geek.Message.Role;

namespace Geek.Hotfix.Logic.Role
{
    public class RoleHeartBeatCompAgent : ComponentAgent<RoleHeartBeatComp>
    {
        public class BeatTimer : TimerHandler<RoleActor>
        {
            protected override async Task HandleTimer(RoleActor actor, Param param)
            {
                var beat = await actor.GetComponent<RoleHeartBeatComp>();
                var now = DateTime.Now;
                if ((now - beat.heartTime).TotalMinutes > 4f)
                {
                    if ((now - beat.activeTime).TotalMinutes > 10f)
                    {
                        //判定断线
                        beat.GetAgentAs<RoleHeartBeatCompAgent>().StopCheck();
                        var role = (await actor.GetComponent<RoleComp>()).State;
                        LOGGER.Info($"心跳超时，视为断线:{role.Name}>{role.RoleId}");
                        var session = SessionManager.Get(role.RoleId);
                        if (session != null)
                        {
                            SessionManager.Remove(session);
                            ChannelUtils.CloseChannel(session.Ctx);
                            await beat.Deactive();
                        }
                    }
                    else
                    {
                        beat.heartTime = now;
                        var msg = new ResHeartBeat();
                        msg.serverTime = now.Ticks;
                        await actor.GetAgentAs<RoleActorAgent>().NotifyClient(msg);
                    }
                }
            }
        }

        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override Task Deactive()
        {
            RemoveTimer(Comp.timerId);
            return Task.CompletedTask;
        }

        public void StartCheck()
        {
            RemoveTimer(Comp.timerId);
            Comp.activeTime = DateTime.Now;
            Comp.timerId = AddTimer<BeatTimer>(3000, 3000);
        }

        public void StopCheck()
        {
            RemoveTimer(Comp.timerId);
        }
        
        public void Hand()
        {
            Comp.activeTime = DateTime.Now;
            Comp.heartTime = Comp.activeTime;
        }
    }
}
