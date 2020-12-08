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
using Geek.Core.Hotfix;
using DotNetty.Transport.Channels;
using Geek.Hotfix.Logic.Role;
using Geek.Hotfix.Logic.Server;
using Geek.App.Login;
using Geek.App.Role;
using Geek.App.Server;
using Geek.App.Session;
using Message.Login;
using System.Threading.Tasks;
using Geek.App.Logic.Login;

namespace Geek.Hotfix.Logic.Login
{
    public enum LoginFailedReason
    {
        TooManyLogin = 1, //登陆人数太多
        AppNotRunning = 2, //正在起服或者关服
        NoChannelId = 3, //channel为空
        UnknownSdkType = 4, //未知的sdk类型
        SeverIdError = 5, //服务器id不匹配
        IPBlock = 6, //ip黑名单
        LoginClose = 7, //登陆未开启
        RegisterClose = 8, //注册关闭
        RoleLoginBlock = 9, //被封号了
        OnlineTop = 10, //在线人数到上限
        RegisterTop = 11, //注册人数到上限
        SdkFailed = 12,
    }

    public class LoginActorAgent : ComponentActorAgent<LoginActor>
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        int uniId;
        IChannelHandlerContext Ctx;
        void KillConnection(LoginFailedReason reason)
        {
            ChannelUtils.SendToClient(Ctx, new ResLogin()
            {
                result = -1,
                reason = (int)reason,
                UniId = uniId,
            });

            Task.Run(async () =>
            {
                await Task.Delay(100);
                await Ctx.CloseAsync();
            });
        }

        public async Task ReqLogin(ReqLogin reqLogin, IChannelHandlerContext ctx)
        {
            this.Ctx = ctx;
            uniId = reqLogin.UniId;

            if(!Settings.Ins.AppRunning)
            {
                KillConnection(LoginFailedReason.AppNotRunning);//正在起服或者关服
                return;
            }

            if (string.IsNullOrEmpty(reqLogin.channelId))
            {
                KillConnection(LoginFailedReason.NoChannelId); //channelId为空
                return;
            }

            //渠道id + 渠道子平台id + 渠道用户名()
            string playerId = reqLogin.sdkType + "_" + reqLogin.channelId + "_" + reqLogin.userName;

            int serverId = reqLogin.serverId;
            if (Settings.Ins.serverId != serverId)
            {
                LOGGER.Error($"serverId不匹配req:{reqLogin.serverId} config:{Settings.Ins.serverId} playerId:{playerId}");
                KillConnection(LoginFailedReason.SeverIdError);
                return;
            }

            var serverActor = await ActorManager.GetOrNew<ServerActor>(ServerActorID.Normal);
            var setting = (await serverActor.GetComponent<ServerSettingComp>());
            var settingAgent = (ServerSettingCompAgent)setting.Agent;
            if(!settingAgent.CanLogin(ctx.Channel.RemoteAddress.ToString()))
            {
                //服务器未开放/ip黑名单
                KillConnection(setting.State.AllowLogin ? LoginFailedReason.IPBlock : LoginFailedReason.LoginClose);
                return;
            }

            var playerAgent = await GetCompAgent<PlayerCompAgent>();
            var playerState = await playerAgent.LoadPlayerState(playerId);
            var roleId = playerAgent.GetCacheRoleId(playerState, serverId, null);

            //如果是断线重连忽略在线人数上限
            var serverAgent = serverActor.GetAgentAs<ServerActorAgent>();
            var onlineNum = await serverAgent.SendAsync(() => serverAgent.GetOnlineNum());
            if (onlineNum >= setting.State.MaxOnlineNum && SessionManager.Get(roleId) == null)
            {
                KillConnection(LoginFailedReason.OnlineTop);
                return;
            }

            if(roleId < 0)
            {
                if (!setting.State.AllowRegister)
                {
                    KillConnection(LoginFailedReason.RegisterClose);
                    return;
                }

                var registerNum = (await serverActor.GetComponent<ServerComp>()).State.RegisterNum;
                if (registerNum >= setting.State.MaxRegisterNum)
                {
                    KillConnection( LoginFailedReason.RegisterTop);
                    return;
                }

                roleId = IdGenerator.GetUniqueId(serverId);
                playerState.roleMap[serverId] = roleId;
            }

            var roleActor = await ActorManager.GetOrNew<RoleActor>(roleId);
            var roleInfo = (await roleActor.GetComponent<RoleComp>()).State;
            if (roleInfo.LoginBlockTime > System.DateTime.Now.Ticks)
            {
                KillConnection(LoginFailedReason.RoleLoginBlock);
                return;
            }

            var isNewRole = roleInfo.CreateTime <= 0;
            if (isNewRole)
            {
                playerState.Id = playerId;
                playerState.userName = reqLogin.userName;
                playerState.sdkType = reqLogin.sdkType;
                playerState.sdkChannel = reqLogin.channelId;
            }

            var session = new Session
            {
                Id = roleId,
                ServerId = serverId,
                Token = reqLogin.handToken,
                Ctx = ctx,
            };
            SessionManager.Add(session);

            LOGGER.Info($"玩家登陆 重连={reqLogin.isRelogin} newRole={isNewRole} player={playerId} role={roleInfo.Name} {roleId}");

            LoginInfo login = new LoginInfo()
            {
                sdkChannel = reqLogin.channelId,
                sdkType = reqLogin.sdkType,
                roleId = roleId,
                playerId = playerId,
                isNewCreate = isNewRole,
                serverId = serverId,
                isReconnect = reqLogin.isRelogin,
            };
            _ = roleActor.GetAgentAs<RoleActorAgent>().OnLogin(Ctx, reqLogin, login, playerState);
        }
    }
}
