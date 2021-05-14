using System.Threading.Tasks;

namespace Geek.Server
{
    public class ServerActorAgent : ComponentActorAgent<ServerActor>
    {
        class CorssDayTimerHandler : TimerHandler<ServerActorAgent>
        {
            protected override async Task HandleTimer(ServerActorAgent actor, Param param)
            {
                //排除时间精度问题
                await Task.Delay(100);
                await actor.CheckCrossDay();
            }
        }

        public override async Task Active()
        {
            await base.Active();
            this.AddDailySchedule<CorssDayTimerHandler>(ServerCompAgent.CrossDayHour, 0);
        }

        public async Task CheckCrossDay()
        {
            var agent = await GetCompAgent<ServerCompAgent>();
            var daysFromOpenServer = agent.GetDaysFromOpenServer();
            if (daysFromOpenServer != agent.State.CacheDaysFromOpenServer)
            {
                agent.State.CacheDaysFromOpenServer = daysFromOpenServer;
            }
        }
    }
}
