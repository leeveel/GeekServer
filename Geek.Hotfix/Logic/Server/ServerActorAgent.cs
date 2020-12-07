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
using Geek.Core.CrossDay;
using Geek.Core.Hotfix;
using Geek.App.Role;
using Geek.App.Server;
using System.Threading.Tasks;

namespace Geek.Hotfix.Logic.Server
{
    public class ServerActorAgent : ComponentActorAgent<ServerActor>, ICrossDayTrigger
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public int GetOnlineNum()
        {
            return Actor.OnLineRoleList.Count;
        }

        public void AddOnlineRole(long roleId)
        {
            if (!Actor.OnLineRoleList.Contains(roleId))
                Actor.OnLineRoleList.Add(roleId);
        }

        public void RemoveOnlineRole(long roleId)
        {
            if (!Actor.OnLineRoleList.Contains(roleId))
                Actor.OnLineRoleList.Add(roleId);
        }




        #region interface ICrossDayLeader
        public async Task<int> GetOpenServerDay()
        {
            var agent = await GetCompAgent<ServerCompAgent>();
            return agent.GetOpenServerDay();
        }

        public async Task<bool> IsNewDay()
        {
            var agent = await GetCompAgent<ServerCompAgent>();
            var day = agent.GetOpenServerDay();
            return day != agent.Comp.State.CacheOpenServerDay;
        }

        public async Task CrossDay(int openServerDay)
        {
            LOGGER.Info("服务器全局跨天:" + openServerDay);
            var comp = await GetComponent<ServerComp>();
            comp.State.CacheOpenServerDay = openServerDay;

            //一般的actor需要在active时再判断一次是否跨天
            //特殊的roleActor不自动跨天 只有在线玩家才执行跨天逻辑
            foreach (var roleId in Actor.OnLineRoleList)
            {
                var roleActor = await ActorManager.GetOrNew<RoleActor>(roleId);
                _ = roleActor.SendAsync(() => roleActor.CrossDay(openServerDay));
            }
        }

        public async Task<int> CheckCrossDay()
        {
            var agent = await GetCompAgent<ServerCompAgent>();
            var day = agent.GetOpenServerDay();
            if (day != agent.Comp.State.CacheOpenServerDay)
            {
                await Actor.CrossDay(day);
                return day;
            }
            return -1;
        }
        #endregion
    }
}