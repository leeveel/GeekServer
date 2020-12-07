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
using Geek.Core.Hotfix;
using Geek.App.Server;
using System.Threading.Tasks;
using Geek.Core.Timer;
using Geek.Core.Events;
using Geek.Hotfix.Events;

namespace Geek.Hotfix.Logic.Server
{
    public class ServerCompAgent : ComponentAgent<ServerComp>, IEventListener
    {
        public class TestEvent : Geek.Core.Events.EventHandler<ServerActor>
        {
            protected override Task HandleEvent(ServerActor actor, Event evt)
            {
                LOGGER.Warn("Test event " + evt.Data + " " + actor);
                return Task.CompletedTask;
            }
        }
        public class Timer2 : TimerHandler<ServerActor>
        {
            protected override Task HandleTimer(ServerActor actor, Param param)
            {
                LOGGER.Warn("Test timer " + param + " " + actor);
                return Task.CompletedTask;
            }
        }

        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override async Task Active()
        {
            await base.Active();
            if (Comp.State.OpenServerTimeTick <= 0)
            {
                LOGGER.Warn("新服开启");
                Comp.State.OpenServerTimeTick = DateTime.Now.Ticks;
                Comp.State.CacheOpenServerDay = 1;
            }

            this.GetActor<ServerActor>().EvtDispatcher.DispatchEvent(EventID.TestEvent, new OneParam<int>(1));
            this.DelayCall<Timer2>(1000, new OneParam<int>(1));
        }

        public int GetOpenServerDay()
        {
            var open = new DateTime(Comp.State.OpenServerTimeTick);
            var now = DateTime.Now;
            if (open.Hour < 0) //开始时间小于跨天时间，相当于昨天开服
                open = new DateTime(open.Year, open.Month, open.Day, 0, 0, 0).AddDays(-1);
            else //开始时间大于跨天时间
                open = new DateTime(open.Year, open.Month, open.Day, 0, 0, 0);
            return (int)Math.Ceiling((now - open).TotalDays);//向上取整
        }

        public void InitEventListener()
        {

            this.GetActor<ServerActor>().EvtDispatcher.AddListener<TestEvent>(EventID.TestEvent);
        }
    }
}
