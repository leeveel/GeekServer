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
using Geek.Core.Component;
using Geek.Core.CrossDay;
using Geek.Core.Events;
using Geek.App.Role;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.App.Server
{
    public class ServerActor : ComponentActor, ICrossDayTrigger
    {
        public List<long> OnLineRoleList = new List<long>();
        /// <summary>serverActor内部事件</summary>
        public EventDispatcher EvtDispatcher { get; private set; }
        /// <summary>serverActor给roleActor派发事件</summary>
        public readonly CrossActorEventDispatcher<RoleActor> RoleEvtDispatcher = new CrossActorEventDispatcher<RoleActor>();
        public override async Task Active()
        {
            EvtDispatcher = new EventDispatcher(this);
            await base.Active();
        }

        public async Task<int> CheckCrossDay()
        {
            if (Agent is ICrossDayTrigger trigger)
                return await SendAsync(trigger.CheckCrossDay);
            else
                throw new Exception("跨天Actor的Agent必须实现ICrossDayTrigger接口");
        }
    }
}