using Geek.Server.Logic.Bag;
using Geek.Server.Message.Login;
using NLog;
using System;
using System.Threading.Tasks;

namespace Geek.Server.Logic.Role
{
    public class RoleCompAgent : StateComponentAgent<RoleComp, RoleState>
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        class RoleEH : EventListener<RoleCompAgent>
        {
            protected override async Task HandleEvent(RoleCompAgent agent, Event evt)
            {
                switch (evt.EventId)
                {
                    case (int)EventID.OnDisconnected:
                        await agent.OnDisconnected();
                        break;
                    case (int)EventID.OnMsgReceived:
                        await agent.OnMsgReceived();
                        break;
                }
            }

            public override Task InitListener(long entityId)
            {
                GED.AddListener<RoleEH>(entityId, EventID.OnDisconnected);
                GED.AddListener<RoleEH>(entityId, EventID.OnMsgReceived);
                return Task.CompletedTask;
            }
        }

        public Task<bool> IsOnline()
        {
            return Task.FromResult(true);
        }

        public Task OnDisconnected()
        {
            return Task.CompletedTask;
        }

        public Task OnMsgReceived()
        {
            //可以用于心跳处理
            return Task.CompletedTask;
        }

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

        [MethodOption.ThreadSafe]
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

        public static Task<bool> IsRoleOnline(long entityId)
        {
            throw new NotImplementedException();
        }

        /// <summary> 通知客户端消息 </summary>
        [MethodOption.NotAwait]
        public virtual async Task NotifyClient(IMessage msg)
        {
            if (await IsOnline())
            {
                var session = SessionManager.Get(EntityId);
                if (session != null)
                {
                    SessionUtils.WriteAndFlush(session.Channel, msg);
                }
            }
        }



        /// <summary>
        /// 此接口是
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="uniId"></param>
        /// <returns></returns>
        [MethodOption.NotAwait]
        public virtual async Task NotifyClient(MSG msg, int uniId = 0)
        {
            if (await IsOnline())
            {
                var session = SessionManager.Get(EntityId);
                if (session != null)
                {
                    if (msg.MsgId > 0)
                        await NotifyClient(msg.msg);

                    if (msg.UniId < 0 || uniId < 0)
                    {
                        return;
                    }

                    var errInfo = msg.Info;
                    ResErrorCode res = new ResErrorCode
                    {
                        UniId = msg.UniId,
                        errCode = (int)errInfo.Code,
                        desc = errInfo.Desc,
                    };
                    if (res.UniId <= 0)
                    {
                        if (msg.msg != null && msg.msg.UniId > 0)
                        {
                            res.UniId = msg.msg.UniId;
                        }
                        else
                        {
                            res.UniId = uniId;
                        }
                    }
                    await NotifyClient(res);
                    if (res.UniId <= 0)
                    {
                        LOGGER.Error($"解锁屏幕消息为0,{msg.msg?.GetType()}");
                    }
                }
            }
        }


    }
}
