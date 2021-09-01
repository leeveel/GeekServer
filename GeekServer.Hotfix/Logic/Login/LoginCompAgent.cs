using DotNetty.Transport.Channels;
using Geek.Server.Logic.Role;
using Geek.Server.Message.Login;
using System;
using System.Threading.Tasks;

namespace Geek.Server.Logic.Login
{
    public class LoginCompAgent : QueryComponentAgent<LoginComp>
    {

        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();


        public async Task Login(IChannelHandlerContext ctx, ReqLogin reqLogin)
        {
            var res = new ResLogin();
            if (string.IsNullOrEmpty(reqLogin.userName))
            {
                res.code = 1; //账号不能为空
                SessionUtils.WriteAndFlush(ctx, res);
                return;
            }

            if (reqLogin.platform != "android" && reqLogin.platform != "ios" && reqLogin.platform != "unity")
            {
                //验证平台合法性
                res.code = 2;
                SessionUtils.WriteAndFlush(ctx, res);
                return;
            }

            if (reqLogin.sdkType > 0)
            {
                //TODO 通过sdktype验证sdktoken
                //可以放到玩家actor
            }

            //验证通过

            //查询角色账号，这里设定每个服务器只能有一个角色
            var roleId = await GetRoleIdOfPlayer(reqLogin.userName, reqLogin.sdkType);
            var isNewRole = roleId <= 0;
            if (isNewRole)
            {
                //没有老角色，创建新号
                roleId = ActorID.NewID(ActorType.Role);
                await CreateRoleToPlayer(reqLogin.userName, reqLogin.sdkType, roleId);
                LOGGER.Info("创建新号:" + roleId);
            }

            //添加到chennel
            var session = new Session();
            session.Id = roleId;
            session.Time = DateTime.Now;
            session.Ctx = ctx;
            session.Sign = reqLogin.device;
            SessionManager.Add(session);

            //登陆流程
            var roleComp = await ActorMgr.GetCompAgent<RoleLoginCompAgent>(roleId);
            await roleComp.OnLogin(reqLogin, isNewRole, roleId);
            var resLogin = await roleComp.BuildLoginMsg();
            SessionUtils.WriteAndFlush(ctx, resLogin);
        }

        public async Task<long> GetRoleIdOfPlayer(string userName, int sdkType)
        {
            var playerId = $"{sdkType}_{userName}";
            if (Comp.PlayerMap.TryGetValue(playerId, out var state))
            {
                if (state.RoleMap.TryGetValue(Settings.Ins.ServerId, out var roleId))
                    return roleId;
                return 0;
            }
            state = await Comp.LoadState(playerId, () =>
            {
                return new PlayerInfoState()
                {
                    Id = playerId,
                    UserName = userName,
                    SdkType = sdkType
                };
            });

            Comp.PlayerMap[playerId] = state;
            if (state.RoleMap.TryGetValue(Settings.Ins.ServerId, out var roleId2))
                return roleId2;
            return 0;
        }

        public Task CreateRoleToPlayer(string userName, int sdkType, long roleId)
        {
            var playerId = $"{sdkType}_{userName}";
            Comp.PlayerMap.TryGetValue(playerId, out var state);
            if (state == null)
            {
                state = new PlayerInfoState();
                state.Id = playerId;
                state.SdkType = sdkType;
                state.UserName = userName;
                Comp.PlayerMap[playerId] = state;
            }
            state.RoleMap[Settings.Ins.ServerId] = roleId;
            return Comp.SaveState(playerId, state);
        }


    }
}
