using Geek.Server.Core.Utils;
using Quartz;

namespace Geek.Server.Core.Timer.Handler
{
    public abstract class NotHotfixTimerHandler : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var param = context.JobDetail.JobDataMap.Get(QuartzTimer.PARAM_KEY) as Param;
            return HandleTimer(param);
        }

        protected abstract Task HandleTimer(Param param);
    }
}