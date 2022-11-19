
using Geek.Server;
using Geek.Server.App.Common;
using Geek.Server.App.Common.Session;
using Geek.Server.App.Logic.Role.Base;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Timer;
using Geek.Server.Proto;
using Server.Logic.Common.Handler;
using Server.Logic.Logic.Role.Bag;

namespace Server.Logic.Logic.Role.Base
{

    public static class RoleCompAgentExt
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public static async Task NotifyClient(this ICompAgent agent, Message msg, int uniId = 0, StateCode code = StateCode.Success)
        {
            var roleComp = await agent.GetCompAgent<RoleCompAgent>();
            if (roleComp != null)
                roleComp.NotifyClient(msg, uniId, code);
            else
                LOGGER.Warn($"{agent.OwnerType}未注册RoleComp组件");
        }
    }

    public class RoleCompAgent : StateCompAgent<RoleComp, RoleState>, ICrossDay
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public async Task<ResLogin> OnLogin(ReqLogin reqLogin, bool isNewRole)
        {
            SetAutoRecycle(false);
            if (isNewRole)
            {
                State.CreateTime = DateTime.Now;
                State.Level = 1;
                State.VipLevel = 1;
                State.RoleName = new System.Random().Next(1000, 10000).ToString();//随机给一个
                //激活背包组件
                await GetCompAgent<BagCompAgent>();
            }
            State.LoginTime = DateTime.Now;
            return BuildLoginMsg();
        }

        public virtual Task OnLogout()
        {
            SetAutoRecycle(true);
            QuartzTimer.Unschedule(ScheduleIdSet);
            return Task.CompletedTask;
        }

        private ResLogin BuildLoginMsg()
        {
            var res = new ResLogin()
            {
                Code = 0,
                UserInfo = new UserInfo()
                {
                    CreateTime = State.CreateTime.Ticks,
                    Level = State.Level,
                    RoleId = State.RoleId,
                    RoleName = State.RoleName,
                    VipLevel = State.VipLevel
                }
            };
            return res;
        }

        Task ICrossDay.OnCrossDay(int openServerDay)
        {
            return Task.CompletedTask;
        }

        public void NotifyClient(Message msg, int uniId=0, StateCode code = StateCode.Success)
        {
            var channel = SessionManager.GetChannel(ActorId);
            if (channel != null && !channel.IsClose())
            {
                channel.WriteAsync(msg, uniId, code);
            }
        }

    }
}
