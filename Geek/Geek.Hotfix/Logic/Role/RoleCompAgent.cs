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
using Geek.Core.Component;
using Geek.Core.Hotfix;
using System;
using Geek.App.Role;
using Geek.App.Server;
using Geek.Hotfix.Logic.Server;
using System.Threading.Tasks;
using Geek.Core.CrossDay;

namespace Geek.Hotfix.Logic.Role
{
    public class RoleCompAgent : ComponentAgent<RoleComp>, ICrossDay
    {
        /// <summary>是否在线</summary>
        public bool IsOnline()
        {
            return Comp.State.OnlineTime > Comp.State.OfflineTime;
        }

        /// <summary>创角天数</summary>
        public int CreateRoleDays()
        {
            return (int)Math.Ceiling((DateTime.Now.Date - new DateTime(Comp.State.CreateTime).Date).TotalDays) + 1;
        }

        /// <summary>离线时间</summary>
        public TimeSpan GetOfflineTime()
        {
            if (!IsOnline())
                return TimeSpan.Zero;
            return new TimeSpan(Comp.State.OfflineTime - Comp.State.OnlineTime);
        }


        public Task OnCrossDay(int openServerDay)
        {
            Comp.State.CacheCrossDay = openServerDay;
            return Task.CompletedTask;
        }

        public async Task OnRoleLogin(LoginInfo login)
        {
            long now = DateTime.Now.Ticks;
            Comp.State.ServerId = login.serverId;
            Comp.State.IsGMRole = login.IsGMRole;
            Comp.State.OnlineTime = now;

            if (login.isNewCreate)
            {
                //新号
                Comp.State.Name = login.newName;
                Comp.State.CreateTime = now;
                Comp.State.VipLevel = 0;
                Comp.State.Level = 1;
                Comp.State.RoleId = login.roleId;
                Comp.State.OrgServerId = login.serverId;
                Comp.State.PlayerId = login.playerId;
                Comp.State.CacheCrossDay = login.openServerDay;
                Comp.State.Name = "new role name";
            }
        }

        public Task OnRoleLogOut()
        {
            Comp.State.OfflineTime = DateTime.Now.Ticks;
            return Task.CompletedTask;
        }
    }
}