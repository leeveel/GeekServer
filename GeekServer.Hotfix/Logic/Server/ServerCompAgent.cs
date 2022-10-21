
using Geek.Server.Server;

namespace Server.Logic.Logic
{
    public class ServerCompAgent : StateCompAgent<ServerComp, ServerState>
    {
        readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        class DelayTimer : TimerHandler<ServerCompAgent>
        {
            protected override Task HandleTimer(ServerCompAgent agent, Param param)
            {
                return agent.TestDelayTimer();
            }
        }

        class ScheduleTimer : TimerHandler<ServerCompAgent>
        {
            protected override Task HandleTimer(ServerCompAgent agent, Param param)
            {
                return agent.TestScheduleTimer();
            }
        }

        public override void Active()
        {
            Delay<DelayTimer>(TimeSpan.FromSeconds(3));
            Schedule<ScheduleTimer>(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
        }

        private Task TestDelayTimer()
        {
            LOGGER.Debug("ServerCompAgent.TestDelayTimer.延时2秒执行.执行一次");
            return Task.CompletedTask;
        }

        private Task TestScheduleTimer()
        {
            LOGGER.Debug("ServerCompAgent.TestSchedueTimer.延时1秒执行.每隔3秒执行");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 由于此接口会提供给其他Actor访问，所以需要标记为[AsyncApi]
        /// </summary>
        /// <returns></returns>
        [AsyncApi]
        public virtual Task<int> GetWorldLevel()
        {
            return Task.FromResult(State.WorldLevel);
        }

    }
}
