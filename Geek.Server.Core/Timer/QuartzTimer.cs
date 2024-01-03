using Geek.Server.Core.Actors;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Timer.Handler;
using Geek.Server.Core.Utils;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace Geek.Server.Core.Timer
{
    public static class QuartzTimer
    {

        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        public static readonly StatisticsTool statisticsTool = new();

        public static void Unschedule(long id)
        {
            if (scheduler.IsShutdown)
                return;
            if (id <= 0)
                return;
            scheduler.DeleteJob(JobKey.Create(id + ""));
        }

        public static void Unschedule(IEnumerable<long> set)
        {
            if (scheduler.IsShutdown)
                return;
            foreach (var id in set)
            {
                if (id > 0)
                    scheduler.DeleteJob(JobKey.Create(id + ""));
            }
        }
        #region 热更定时器
        /// <summary>
        /// 每隔一段时间执行一次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actorId"></param>
        /// <param name="firstTime"></param>
        /// <param name="interval"></param>
        /// <param name="param"></param>
        /// <param name="repeatCount"> -1 表示永远 </param>
        /// <returns></returns>
        public static long Schedule<T>(long actorId, TimeSpan delay, TimeSpan interval, Param param = null, int repeatCount = -1) where T : ITimerHandler
        {
            var id = NextId();
            var firstTimeOffset = DateTimeOffset.Now.Add(delay);
            TriggerBuilder builder;
            if (repeatCount < 0)
            {
                builder = TriggerBuilder.Create().StartAt(firstTimeOffset).WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever());
            }
            else
            {
                builder = TriggerBuilder.Create().StartAt(firstTimeOffset).WithSimpleSchedule(x => x.WithInterval(interval).WithRepeatCount(repeatCount));
            }
            scheduler.ScheduleJob(GetJobDetail<T>(id, actorId, param), builder.Build());
            return id;
        }

        /// <summary>
        /// 基于时间delay
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actorId"></param>
        /// <param name="executeTime"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static long Delay<T>(long actorId, TimeSpan delay, Param param = null) where T : ITimerHandler
        {
            var id = NextId();
            var firstTimeOffset = DateTimeOffset.Now.Add(delay);
            var trigger = TriggerBuilder.Create().StartAt(firstTimeOffset).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, actorId, param), trigger);
            return id;
        }

        /// <summary>
        /// 基于cron表达式
        /// </summary>
        public static long WithCronExpression<T>(long actorId, string cronExpression, Param param = null) where T : ITimerHandler
        {
            var id = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithCronSchedule(cronExpression).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, actorId, param), trigger);
            return id;
        }

        /// <summary>
        /// 每日
        /// </summary>
        public static long Daily<T>(long actorId, int hour, int minute, Param param = null) where T : ITimerHandler
        {
            if (hour < 0 || hour >= 24 || minute < 0 || minute >= 60)
            {
                throw new ArgumentOutOfRangeException($"定时器参数错误 TimerHandler:{typeof(T).FullName} {nameof(hour)}:{hour} {nameof(minute)}:{minute}");
            }
            var id = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(hour, minute)).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, actorId, param), trigger);
            return id;
        }

        /// <summary>
        /// 每周某些天
        /// </summary>
        public static long WithDayOfWeeks<T>(long actorId, int hour, int minute, Param param, params DayOfWeek[] dayOfWeeks) where T : ITimerHandler
        {
            if (dayOfWeeks == null || dayOfWeeks.Length <= 0)
            {
                throw new ArgumentNullException($"定时每周执行 参数为空：{nameof(dayOfWeeks)} TimerHandler:{typeof(T).FullName} actorId:{actorId} actorType:{IdGenerator.GetActorType(actorId)}");
            }
            var id = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(hour, minute, dayOfWeeks)).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, actorId, param), trigger);
            return id;
        }

        /// <summary>
        /// 每周某天
        /// </summary>
        public static long Weekly<T>(long actorId, DayOfWeek dayOfWeek, int hour, int minute, Param param = null) where T : ITimerHandler
        {
            var id = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(dayOfWeek, hour, minute)).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, actorId, param), trigger);
            return id;
        }

        /// <summary>
        /// 每月某天
        /// </summary>
        public static long Monthly<T>(long actorId, int dayOfMonth, int hour, int minute, Param param = null) where T : ITimerHandler
        {
            if (dayOfMonth < 0 || dayOfMonth > 31)
            {
                throw new ArgumentException($"定时器参数错误 TimerHandler:{typeof(T).FullName} {nameof(dayOfMonth)}:{dayOfMonth} actorId:{actorId} actorType:{IdGenerator.GetActorType(actorId)}");
            }
            var id = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(dayOfMonth, hour, minute)).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, actorId, param), trigger);
            return id;
        }
        #endregion

        #region 非热更定时器
        /// <summary>
        /// 每隔一段时间执行一次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actorId"></param>
        /// <param name="firstTime"></param>
        /// <param name="interval"></param>
        /// <param name="param"></param>
        /// <param name="repeatCount"> -1 表示永远 </param>
        /// <returns></returns>
        public static long Schedule<T>(TimeSpan delay, TimeSpan interval, Param param = null, int repeatCount = -1) where T : NotHotfixTimerHandler
        {
            var id = NextId();
            var firstTimeOffset = DateTimeOffset.Now.Add(delay);
            TriggerBuilder builder;
            if (repeatCount < 0)
            {
                builder = TriggerBuilder.Create().StartAt(firstTimeOffset).WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever());
            }
            else
            {
                builder = TriggerBuilder.Create().StartAt(firstTimeOffset).WithSimpleSchedule(x => x.WithInterval(interval).WithRepeatCount(repeatCount));
            }
            scheduler.ScheduleJob(GetJobDetail<T>(id, param), builder.Build());
            return id;
        }

        /// <summary>
        /// 基于时间delay
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actorId"></param>
        /// <param name="executeTime"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static long Delay<T>(TimeSpan delay, Param param = null) where T : NotHotfixTimerHandler
        {
            var id = NextId();
            var firstTimeOffset = DateTimeOffset.Now.Add(delay);
            var trigger = TriggerBuilder.Create().StartAt(firstTimeOffset).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, param), trigger);
            return id;
        }

        /// <summary>
        /// 基于cron表达式
        /// </summary>
        public static long WithCronExpression<T>(string cronExpression, Param param = null) where T : NotHotfixTimerHandler
        {
            var id = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithCronSchedule(cronExpression).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, param), trigger);
            return id;
        }

        /// <summary>
        /// 每日
        /// </summary>
        public static long Daily<T>(int hour, int minute, Param param = null) where T : NotHotfixTimerHandler
        {
            if (hour < 0 || hour >= 24 || minute < 0 || minute >= 60)
            {
                throw new ArgumentOutOfRangeException($"定时器参数错误 TimerHandler:{typeof(T).FullName} {nameof(hour)}:{hour} {nameof(minute)}:{minute}");
            }
            var id = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(hour, minute)).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, param), trigger);
            return id;
        }

        /// <summary>
        /// 每周某些天
        /// </summary>
        public static long WithDayOfWeeks<T>(int hour, int minute, Param param, params DayOfWeek[] dayOfWeeks) where T : NotHotfixTimerHandler
        {
            if (dayOfWeeks == null || dayOfWeeks.Length <= 0)
            {
                throw new ArgumentNullException($"定时每周执行 参数为空：{nameof(dayOfWeeks)} TimerHandler:{typeof(T).FullName}");
            }
            var id = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(hour, minute, dayOfWeeks)).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, param), trigger);
            return id;
        }

        /// <summary>
        /// 每周某天
        /// </summary>
        public static long Weekly<T>(DayOfWeek dayOfWeek, int hour, int minute, Param param = null) where T : NotHotfixTimerHandler
        {
            var id = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(dayOfWeek, hour, minute)).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, param), trigger);
            return id;
        }

        /// <summary>
        /// 每月某天
        /// </summary>
        public static long Monthly<T>(int dayOfMonth, int hour, int minute, Param param = null) where T : NotHotfixTimerHandler
        {
            if (dayOfMonth < 0 || dayOfMonth > 31)
            {
                throw new ArgumentException($"定时器参数错误 TimerHandler:{typeof(T).FullName} {nameof(dayOfMonth)}:{dayOfMonth}");
            }
            var id = NextId();
            var trigger = TriggerBuilder.Create().StartNow().WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(dayOfMonth, hour, minute)).Build();
            scheduler.ScheduleJob(GetJobDetail<T>(id, param), trigger);
            return id;
        }
        #endregion

        #region 调度
        static IScheduler scheduler = null;

        /// <summary>
        /// 可防止反复初始化
        /// </summary>
        static QuartzTimer()
        {
            Init().Wait();
        }

        static async Task Init()
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
            var factory = new StdSchedulerFactory();
            scheduler = await factory.GetScheduler();
            await scheduler.Start();
        }

        public static Task Stop()
        {
            return scheduler.Shutdown();
        }

        private static long id = DateTime.Now.Ticks;
        private static long NextId()
        {
            return Interlocked.Increment(ref id);
        }

        class TimerJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                var handlerType = context.JobDetail.JobDataMap.GetString(TIMER_KEY);
                try
                {
                    var param = context.JobDetail.JobDataMap.Get(PARAM_KEY) as Param;
                    var handler = HotfixMgr.GetInstance<ITimerHandler>(handlerType);
                    if (handler != null)
                    {
                        var actorId = context.JobDetail.JobDataMap.GetLong(ACTOR_ID_KEY);
                        var agentType = handler.GetType().BaseType.GenericTypeArguments[0];
                        var comp = await ActorMgr.GetCompAgent(actorId, agentType);
                        comp.Tell(() => handler.InnerHandleTimer(comp, param));

                    }
                    else
                    {
                        Log.Error($"错误的ITimer类型，回调失败 type:{handlerType}");
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }

        public const string PARAM_KEY = "param";
        const string ACTOR_ID_KEY = "actor_id";
        const string TIMER_KEY = "timer";

        private static IJobDetail GetJobDetail<T>(long id, long actorId, Param param) where T : ITimerHandler
        {
            var handlerType = typeof(T);
            statisticsTool.Count(handlerType.FullName);
            if (handlerType.Assembly != HotfixMgr.HotfixAssembly)
            {
                throw new Exception("定时器代码需要在热更项目里");
            }
            var job = JobBuilder.Create<TimerJob>().WithIdentity(id + "").Build();
            job.JobDataMap.Add(PARAM_KEY, param);
            job.JobDataMap.Add(ACTOR_ID_KEY, actorId);
            job.JobDataMap.Add(TIMER_KEY, handlerType.FullName);
            return job;
        }

        private static IJobDetail GetJobDetail<T>(long id, Param param) where T : NotHotfixTimerHandler
        {
            statisticsTool.Count(typeof(T).FullName);
            var job = JobBuilder.Create<T>().WithIdentity(id + "").Build();
            job.JobDataMap.Add(PARAM_KEY, param);
            return job;
        }

        class ConsoleLogProvider : ILogProvider
        {
            public Logger GetLogger(string name)
            {
                return (level, func, exception, parameters) =>
                {
                    if (func != null)
                    {
                        if (level < LogLevel.Warn)
                        {

                        }
                        else if (level == LogLevel.Warn)
                        {
                            Log.Warn(func(), parameters);
                        }
                        else if (level > LogLevel.Warn)
                        {
                            Log.Error(func(), parameters);
                        }
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
        #endregion
    }
}
