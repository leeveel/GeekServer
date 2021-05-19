using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Geek.Server
{
    public sealed class QuartzTimer
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        static IScheduler scheduler;

        const string ParamKey = "param";
        const string ActorIdKey = "actorId";
        const string ActorAgentTypeKey = "actorType";
        const string HandlerTypeKey = "handlerType";

        class TimerJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                var actorId = context.JobDetail.JobDataMap.GetLong(ActorIdKey);
                var actorType = context.JobDetail.JobDataMap.GetString(ActorAgentTypeKey);
                var handlerType = context.JobDetail.JobDataMap.GetString(HandlerTypeKey);

                var param = context.JobDetail.JobDataMap.Get(ParamKey) as Param;
                await triggerTimer(actorId, actorType, handlerType, param);
            }
        }

        static async Task triggerTimer(long actorId, string actorType, string handlerType, Param param)
        {
            try
            {
                var handler = HotfixMgr.GetInstance<ITimerHandler>(handlerType);
                var agentType = handler.GetType().BaseType.GenericTypeArguments[0];
                if (agentType.GetInterface(typeof(IComponentActorAgent).FullName) != null)
                {
                    //actor
                    var agent = await ActorManager.GetOrNew(agentType, actorId);
                    var actor = (ComponentActor)agent.Owner;
                    _ = actor.SendAsync(() => handler.InternalHandleTimer(agent, param), false);
                }
                else if (agentType.GetInterface(typeof(IComponentAgent).FullName) != null)
                {
                    //component
                    var actorAgentType = HotfixMgr.GetType(actorType, agentType);
                    var compType = agentType.BaseType.GenericTypeArguments[0];
                    var agent = await ActorManager.GetOrNew(actorAgentType, actorId);
                    var actor = (ComponentActor)agent.Owner;
                    var comp = await actor.GetComponent(compType);
                    _ = actor.SendAsync(() => handler.InternalHandleTimer(comp.GetAgent(agentType), param), false);
                }
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
        }

        class ConsoleLogProvider : ILogProvider
        {
            public Logger GetLogger(string name)
            {
                return (level, func, exception, parameters) =>
                {
                    if(func != null)
                    {
                        if (level < LogLevel.Warn)
                        { }//LOGGER.Debug(func(), parameters);
                        else if (level == LogLevel.Warn)
                            LOGGER.Warn(func(), parameters);
                        else if (level > LogLevel.Warn)
                            LOGGER.Error(func(), parameters);
                    }
                    return true;
                };
            }

            public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
            {
                throw new NotImplementedException();
            }

            public IDisposable OpenNestedContext(string message)
            {
                throw new NotImplementedException();
            }
        }

        static QuartzTimer()
        {
            initScheduler();
        }

        static async void initScheduler()
        {
            if (scheduler != null)
                return;

            //使用mongodb持久化参考文档[全量]
            //https://www.cnblogs.com/huangxincheng/p/7078895.html

            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
            var factory = new StdSchedulerFactory();
            scheduler = await factory.GetScheduler();
            await scheduler.Start();
        }

        static long idNow;
        static long newID()
        {
            return Interlocked.Increment(ref idNow);
        }

        public static void Remove(long id)
        {
            scheduler.DeleteJob(JobKey.Create(id + ""));
        }

        static IJobDetail getJobDetail(long id, long actorId, string actorAgentType, string handlerType, Param param)
        {
            var job = JobBuilder.Create<TimerJob>().WithIdentity(id + "").Build();
            job.JobDataMap.Add(ParamKey, param);
            job.JobDataMap.Add(ActorIdKey, actorId);
            job.JobDataMap.Add(ActorAgentTypeKey, actorAgentType);
            job.JobDataMap.Add(HandlerTypeKey, handlerType);
            return job;
        }
        
        public static long AddDelay(long delay, long actorId, string actorAgentType, string handlerType, Param param)
        {
            long id = newID();
            var delayTime = DateTimeOffset.Now.AddMilliseconds(delay);
            var trigger = TriggerBuilder.Create().StartAt(delayTime).Build();
            scheduler.ScheduleJob(getJobDetail(id, actorId, actorAgentType, handlerType, param), trigger);
            return id;
        }

        public static long AddTimer(long delay, long period, long actorId, string actorAgentType, string handlerType, Param param)
        {
            long id = newID();
            var delayTime = DateTimeOffset.Now.AddMilliseconds(delay);
            var trigger = TriggerBuilder.Create().StartAt(delayTime).WithSimpleSchedule(x=>x.WithInterval(TimeSpan.FromMilliseconds(period)).RepeatForever()).Build();
            scheduler.ScheduleJob(getJobDetail(id, actorId, actorAgentType, handlerType, param), trigger);
            return id;
        }

        /// <summary>
        /// 一次性
        /// </summary>
        public static long AddOnceSchedule(DateTime time, long actorId, string actorAgentType, string handlerType, Param param)
        {
            long id = newID();
            var delta = time - DateTime.Now;
            var delayTime = DateTimeOffset.Now.Add(delta);
            var trigger = TriggerBuilder.Create().StartAt(delayTime).Build();
            scheduler.ScheduleJob(getJobDetail(id, actorId, actorAgentType, handlerType, param), trigger);
            return id;
        }

        /// <summary>
        /// 每天
        /// </summary>
        public static long AddDailySchedule(int hour, int minute, long actorId, string actorAgentType, string handlerType, Param param)
        {
            long id = newID();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(hour, minute)).Build();
            scheduler.ScheduleJob(getJobDetail(id, actorId, actorAgentType, handlerType, param), trigger);
            return id;
        }

        /// <summary>
        /// 每周
        /// </summary>
        public static long AddWeeklySchedule(DayOfWeek[] days, int hour, int minute, long actorId, string actorAgentType, string handlerType, Param param)
        {
            long id = newID();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(hour, minute, days)).Build();
            scheduler.ScheduleJob(getJobDetail(id, actorId, actorAgentType, handlerType, param), trigger);
            return id;
        }

        /// <summary>
        /// 每月
        /// </summary>
        public static long AddMonthlySchedule(int dayOfMonth, int hour, int minute, long actorId, string actorAgentType, string handlerType, Param param)
        {
            long id = newID();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(dayOfMonth, hour, minute)).Build();
            scheduler.ScheduleJob(getJobDetail(id, actorId, actorAgentType, handlerType, param), trigger);
            return id;
        }

        public static Task Stop()
        {
            return scheduler.Shutdown();
        }
    }
}
