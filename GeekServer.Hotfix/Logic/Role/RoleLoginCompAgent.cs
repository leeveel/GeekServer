using Geek.Server.Logic.Bag;
using Geek.Server.Message.Login;
using System;
using System.Threading.Tasks;

namespace Geek.Server.Logic.Role
{
    public class RoleLoginCompAgent : StateComponentAgent<RoleLoginComp, RoleInfoState>
    {

        public Task OnCreate(ReqLogin reqLogin, long roleId)
        {
            State.CreateTime = DateTime.Now;
            State.Level = 1;
            State.VipLevel = 1;
            State.RoleId = roleId;
            State.RoleName = new System.Random().Next(1000, 10000).ToString();//随机给一个
            return Task.CompletedTask;
        }

        public async Task OnLogin(ReqLogin reqLogin, bool isNewRole, long roleId)
        {
            if (isNewRole)
            {
                await OnCreate(reqLogin, roleId);
                var bagComp = await GetCompAgent<BagCompAgent>();
                await bagComp.Init();
            }
            State.LoginTime = DateTime.Now;
        }

        public Task OnLoginOut()
        {
            State.OfflineTime = DateTime.Now;
            return Task.CompletedTask;
        }

        [ThreadSafe]
        public Task<ResLogin> BuildLoginMsg()
        {
            var res = new ResLogin()
            {
                code = 0,
                userInfo = new UserInfo()
                {
                    createTime = State.CreateTime.Ticks,
                    level = State.Level,
                    roleId = State.RoleId,
                    roleName = State.RoleName,
                    vipLevel = State.VipLevel
                }
            };
            return Task.FromResult(res);
        }

    }
}
