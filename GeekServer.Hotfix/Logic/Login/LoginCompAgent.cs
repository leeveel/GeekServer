using DotNetty.Transport.Channels;
using Geek.Server.Logic.Role;
using Geek.Server.Proto;
using System;
using System.Threading.Tasks;

namespace Geek.Server.Logic.Login
{
    public class LoginCompAgent : QueryComponentAgent<LoginComp>
    {

        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public virtual async Task<MSG> Login(IChannel channel, ReqLogin reqLogin)
        {
            if (string.IsNullOrEmpty(reqLogin.UserName))
            {
                return MSG.Create(ErrCode.AccountCannotBeNull);
            }

            if (reqLogin.Platform != "android" && reqLogin.Platform != "ios" && reqLogin.Platform != "unity")
            {
                //验证平台合法性
                return MSG.Create(ErrCode.UnknownPlatform);
            }

            if (reqLogin.SdkType > 0)
            {
                //TODO 通过sdktype验证sdktoken
                //可以放到玩家actor
            }

            //验证通过

            //查询角色账号，这里设定每个服务器只能有一个角色
            var roleId = await GetRoleIdOfPlayer(reqLogin.UserName, reqLogin.SdkType);
            var isNewRole = roleId <= 0;
            if (isNewRole)
            {
                //没有老角色，创建新号
                roleId = EntityID.NewID(EntityType.Role);
                await CreateRoleToPlayer(reqLogin.UserName, reqLogin.SdkType, roleId);
                LOGGER.Info("创建新号:" + roleId);
            }

            //添加到session
            var session = new Session
            {
                Id = roleId,
                Time = DateTime.Now,
                Channel = channel,
                Sign = reqLogin.Device
            };
            SessionManager.Add(session);

            //登陆流程
            var roleComp = await EntityMgr.GetCompAgent<RoleCompAgent>(roleId);
            await roleComp.OnLogin(reqLogin, isNewRole, roleId);
            var resLogin = await roleComp.BuildLoginMsg();
            return MSG.Create(resLogin, reqLogin.UniId);
        }

        public virtual async Task<long> GetRoleIdOfPlayer(string userName, int sdkType)
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
                var playerState = (PlayerInfoState)BaseDBState.CreateStateWrapper<PlayerInfoState>();
                playerState.Id = playerId;
                playerState.UserName = userName;
                playerState.SdkType = sdkType;
                return playerState;
            });

            Comp.PlayerMap[playerId] = state;
            if (state.RoleMap.TryGetValue(Settings.Ins.ServerId, out var roleId2))
                return roleId2;
            return 0;
        }

        public virtual Task CreateRoleToPlayer(string userName, int sdkType, long roleId)
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
