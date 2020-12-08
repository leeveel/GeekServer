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
using Geek.App.Role;
using Geek.Core.Hotfix;
using System.Threading.Tasks;
using Geek.Hotfix.Events;
using Geek.Core.Actor;
using Geek.App.Server;
using Geek.Core.Net.Message;
using Geek.App.Session;
using Geek.App.Login;
using Geek.Hotfix.Logic.Server;
using Geek.Hotfix.Logic.Login;
using Geek.Core.Component;
using DotNetty.Transport.Channels;
using Geek.App.Logic.Login;
using Geek.Message.Login;

namespace Geek.Hotfix.Logic.Role
{
    public class RoleActorAgent : ComponentActorAgent<RoleActor>, ISession, IDeadable
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override async Task Active()
        {
            Actor.AutoCrossDay = false;
            Actor.AutoRecycleEnable = true;
            await base.Active();
            var roleComp = await GetComponent<RoleComp>();
            if (roleComp.State.ServerId != Settings.Ins.serverId)
                Actor.ReadOnly = true;
        }

        public Task OnLogin(IChannelHandlerContext ctx, ReqLogin reqLogin, LoginInfo login, PlayerState player)
        {
            return SendAsync(async () =>
            {
                var serverActor = await ActorManager.GetOrNew<ServerActor>(ServerActorID.Normal);
                var serverState = await serverActor.SendAsync(async () => {
                    var serverComp = await serverActor.GetComponent<ServerComp>();
                    //注册人数,放在这里注册数可能略高于注册上限
                    if (login.isNewCreate)
                        serverComp.State.RegisterNum++;

                    //加入在线玩家,在线数可能略高于在线上限
                    var serverAgent = serverActor.GetAgentAs<ServerActorAgent>();
                    serverAgent.AddOnlineRole(login.roleId);
                    return serverComp.State;
                });

                login.openServerDay = serverState.CacheOpenServerDay;
                var com = await GetCompAgent<RoleCompAgent>();
                await com.OnRoleLogin(login);

                if (login.isNewCreate)
                {
                    //playerState的读写都在登陆actor
                    var loginActor = await ActorManager.GetOrNew<LoginActor>(ServerActorID.Login);
                    _ = Task.Run(() => { //避开死锁检测
                        loginActor.SendAsync(async () => {
                            var playerComp = await loginActor.GetComponent<PlayerComp>();
                            var playerAgent = playerComp.GetAgentAs<PlayerCompAgent>();
                            await playerAgent.SavePlayerState(player);
                        });
                    });
                }

                Actor.ReadOnly = false;
                Actor.AutoRecycleEnable = false;
                await Actor.InitListener();
                (await GetCompAgent<RoleHeartBeatCompAgent>()).StartCheck(); //心跳检测

                await loginCheckCrossDay();
                Actor.EvtDispatcher.DispatchEvent(EventID.OnRoleLogin);
            });
        }

        async Task loginCheckCrossDay()
        {
            var state = (await GetComponent<RoleComp>()).State;
            var serverActor = await ActorManager.GetOrNew<ServerActor>(ServerActorID.Normal);
            var serverOpenDay = await serverActor.SendAsync(async () =>
            {
                var serverComp = await serverActor.GetComponent<ServerComp>();
                return serverComp.State.CacheOpenServerDay;
            });

            if (serverOpenDay > state.CacheCrossDay)
            {
                LOGGER.Info("登陆触发跨天");
                await Actor.CrossDay(serverOpenDay);
            }
        }

        async Task OnLoginOut()
        {
            var agent = await GetCompAgent<RoleCompAgent>();
            await agent.OnRoleLogOut();
            (await GetCompAgent<RoleHeartBeatCompAgent>()).StopCheck();
            Actor.EvtDispatcher.DispatchEvent(EventID.OnRoleOffline);
            Actor.AutoRecycleEnable = true;
            var serverActor = await ActorManager.GetOrNew<ServerActor>(ServerActorID.Normal);
            await serverActor.SendAsync(() =>{
                serverActor.GetAgentAs<ServerActorAgent>().RemoveOnlineRole(Actor.ActorId); 
            });
        }

        public async Task Die()
        {
            var serverActor = await ActorManager.GetOrNew<ServerActor>(ServerActorID.Normal);
            serverActor.RoleEvtDispatcher.ClearListener(Actor.ActorId);
        }

        /// <summary>
        /// 是否在线
        /// </summary>
        public async Task<bool> IsOnline()
        {
            var roleAgent = (await GetCompAgent<RoleCompAgent>());
            return roleAgent.IsOnline();
        }

        public async Task NotifyClient(IMessage msg)
        {
            if (await this.IsOnline())
            {
                var role = await GetComponent<RoleComp>();
                var session = SessionManager.Get(role.State.RoleId);
                if (session != null)
                {
                    ChannelUtils.SendToClient(session.Ctx, msg);
                    //ResCode.OnSendMsg(session.Ctx, msg.Id);
                }
            }
        }
        
        public void OnDisConnect()
        {
            //下线处理
            SendAsync(async ()=> {
                if(await IsOnline())
                    await OnLoginOut();
            });
        }

        public void Hand()
        {
            //握手
            SendAsync(async ()=>{ 
                var beat = await GetCompAgent<RoleHeartBeatCompAgent>();
                beat.Hand();
            });
        }
    }
}