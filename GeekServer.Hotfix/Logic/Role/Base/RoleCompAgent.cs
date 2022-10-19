
namespace Geek.Server.Role
{
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
                var bagComp = await GetCompAgent<BagCompAgent>();
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
    }
}
