using System;
using System.Threading.Tasks;
using Geek.Server;

namespace Geek.Server.Logic.Server
{
    public class ServerCompAgent : StateComponentAgent<ServerComp, ServerState>
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public override async Task Active()
        {
            await base.Active();
            this.Schedule<CorssDayTimerHandler>(ServerComp.CrossDayHour, 0);
            if (State.OpenServerTimeTick <= 0)
            {
                State.OpenServerTimeTick = DateTime.Now.Ticks;
                LOGGER.Warn("新服开启");
            }
        }

        class CorssDayTimerHandler : TimerHandler<ServerCompAgent>
        {
            protected override async Task HandleTimer(ServerCompAgent agent, Param param)
            {
                //排除时间精度问题,Quartz可能产生1,2毫秒误差
                await Task.Delay(100);
                _ = agent.CheckCrossDay();
            }
        }

        [MethodOption.NotAwait]
        public virtual async Task CheckCrossDay()
        {
            var daysFromOpenServer = await GetDaysFromOpenServer();
            if (daysFromOpenServer != State.CacheDaysFromOpenServer)
            {
                LOGGER.Info("开服天数变化 当前:{}天", daysFromOpenServer);
                State.CacheDaysFromOpenServer = daysFromOpenServer;
                //处理跨天逻辑
                //.....
                //.....
            }
        }

        /// <summary>
        /// 开服天数
        /// </summary>
        public virtual Task<int> GetDaysFromOpenServer()
        {
            var open = new DateTime(State.OpenServerTimeTick);
            var now = DateTime.Now;
            if (open.Hour < ServerComp.CrossDayHour) //开始时间小于跨天时间，相当于昨天开服
                open = new DateTime(open.Year, open.Month, open.Day, ServerComp.CrossDayHour, 0, 0).AddDays(-1);
            else //开始时间大于跨天时间
                open = new DateTime(open.Year, open.Month, open.Day, ServerComp.CrossDayHour, 0, 0);
            return Task.FromResult((int)Math.Ceiling((now - open).TotalDays));//向上取整
        }

    }
}
