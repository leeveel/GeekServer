using Geek.Server.Logic.Bag;
using Geek.Server.Proto;
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

        public virtual Task<bool> IsOnline()
        {
            return Task.FromResult(true);
        }

        public virtual Task OnDisconnected()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnMsgReceived()
        {
            //可以用于心跳处理
            return Task.CompletedTask;
        }

        public virtual Task OnCreate(ReqLogin reqLogin, long roleId)
        {
            State.CreateTime = DateTime.Now;
            State.Level = 1;
            State.VipLevel = 1;
            State.RoleId = roleId;
            State.RoleName = new System.Random().Next(1000, 10000).ToString();//随机给一个
            return Task.CompletedTask;
        }

        public async virtual Task OnLogin(ReqLogin reqLogin, bool isNewRole, long roleId)
        {
            if (isNewRole)
            {
                await OnCreate(reqLogin, roleId);
                var bagComp = await GetCompAgent<BagCompAgent>();
                await bagComp.Init();
            }
            State.LoginTime = DateTime.Now;
        }

        public virtual Task OnLoginOut()
        {
            State.OfflineTime = DateTime.Now;
            return Task.CompletedTask;
        }

        [MethodOption.ThreadSafe]
        public virtual Task<ResLogin> BuildLoginMsg()
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
                        return;

                    var errInfo = msg.Info;
                    ResErrorCode res = new ResErrorCode
                    {
                        UniId = msg.UniId,
                        ErrCode = (int)errInfo.Code,
                        Desc = errInfo.Desc,
                    };
                    await NotifyClient(res);
                }
            }
        }


    }
}
