using Geek.Server.App.Login;
using Geek.Server.App.Net.Session;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Utils;
using Geek.Server.Hotfix.Role.Base;
using Geek.Server.Hotfix.Server;

namespace Geek.Server.Hotfix.Login
{
    public class LoginCompAgent : StateCompAgent<LoginComp, LoginState>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public async Task OnLogin(GameSession session, ReqLogin reqLogin)
        {
            if (string.IsNullOrEmpty(reqLogin.UserName))
            {
                session.WriteAsync(null, reqLogin.UniId, StateCode.AccountCannotBeNull);
            }

            if (reqLogin.Platform != "android" && reqLogin.Platform != "ios" && reqLogin.Platform != "unity")
            {
                //验证平台合法性
                session.WriteAsync(null, reqLogin.UniId, StateCode.UnknownPlatform);
            }

            //查询角色账号，这里设定每个服务器只能有一个角色
            var roleId = GetRoleIdOfPlayer(reqLogin.UserName, reqLogin.SdkType);
            var isNewRole = roleId <= 0;
            if (isNewRole)
            {
                //没有老角色，创建新号
                roleId = IdGenerator.GetActorID(ActorType.Role);
                CreateRoleToPlayer(reqLogin.UserName, reqLogin.SdkType, roleId);
                Log.Info("创建新号:" + roleId);
            }
            else
            {
                Log.Info("老号登录:" + roleId);
            }

            //添加到session 
            session.RoleId = roleId;
            session.Sign = reqLogin.Sign;
            SessionManager.Add(session);

            //登陆流程
            var roleComp = await ActorMgr.GetCompAgent<RoleCompAgent>(roleId);
            //从登录线程-->调用Role线程 所以需要入队
            var resLogin = await roleComp.OnLogin(reqLogin, isNewRole);
            session.WriteAsync(resLogin, reqLogin.UniId, StateCode.Success);

            var serverComp = await ActorMgr.GetCompAgent<ServerCompAgent>();
            //加入在线玩家
            await serverComp.AddOnlineRole(ActorId);
        }

        private long GetRoleIdOfPlayer(string userName, int sdkType)
        {
            var playerId = $"{sdkType}_{userName}";
            if (State.PlayerMap.TryGetValue(playerId, out var state))
            {
                if (state.RoleMap.TryGetValue(Settings.ServerId, out var roleId))
                    return roleId;
                return 0;
            }
            return 0;
        }

        private void CreateRoleToPlayer(string userName, int sdkType, long roleId)
        {
            var playerId = $"{sdkType}_{userName}";
            State.PlayerMap.TryGetValue(playerId, out var info);
            if (info == null)
            {
                info = new PlayerInfo();
                info.playerId = playerId;
                info.SdkType = sdkType;
                info.UserName = userName;
                State.PlayerMap[playerId] = info;
            }
            info.RoleMap[Settings.ServerId] = roleId;
        }

    }
}