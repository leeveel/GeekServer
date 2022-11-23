
using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Timer;
using Geek.Server.Core.Timer.Handler;
using Geek.Server.Core.Utils;

namespace Geek.Server.Core.Hotfix.Agent
{
    public abstract class BaseCompAgent<TComp> : ICompAgent where TComp : BaseComp
    {
        public BaseComp Owner { get; set; }
        public TComp Comp => (TComp)Owner;
        public Actor Actor => Owner.Actor;
        public long ActorId => Actor.Id;
        public ActorType OwnerType => Actor.Type;

        public HashSet<long> ScheduleIdSet => Actor.ScheduleIdSet;

        public virtual void Active()
        {

        }

        protected void SetAutoRecycle(bool autoRecycle)
        {
            Actor.SetAutoRecycle(autoRecycle);
        }

        public virtual Task Deactive()
        {
            return Task.CompletedTask;
        }

        public Task ActorCrossDay(int serverDay)
        {
            return Actor.CrossDay(serverDay);
        }

        public Task<ICompAgent> GetCompAgent(Type agentType)
        {
            return Actor.GetCompAgent(agentType);
        }

        public Task<T> GetCompAgent<T>() where T : ICompAgent
        {
            return Actor.GetCompAgent<T>();
        }

        public void Tell(Action work, int timeout = Actor.TIME_OUT)
        {
            Actor.Tell(work, timeout);
        }

        public void Tell(Func<Task> work, int timeout = Actor.TIME_OUT)
        {
            Actor.Tell(work, timeout);
        }

        public Task SendAsync(Action work, int timeout = Actor.TIME_OUT)
        {
            return Actor.SendAsync(work, timeout);
        }

        public Task<T> SendAsync<T>(Func<T> work, int timeout = Actor.TIME_OUT)
        {
            return Actor.SendAsync(work, timeout);
        }

        public Task SendAsync(Func<Task> work, int timeout = Actor.TIME_OUT)
        {
            return Actor.SendAsync(work, timeout);
        }

        public Task<T> SendAsync<T>(Func<Task<T>> work, int timeOut = Actor.TIME_OUT)
        {
            return Actor.SendAsync(work, timeOut);
        }

        public void Unschedule(long id)
        {
            QuartzTimer.Unschedule(id);
            ScheduleIdSet.Remove(id);
        }

        public long Delay<T>(DateTime time, Param param = null, long unscheduleId = 0) where T : ITimerHandler
        {
            Unschedule(unscheduleId);
            long scheduleId = QuartzTimer.Delay<T>(ActorId, time - DateTime.Now, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        public long Delay<T>(long time, Param param = null, long unscheduleId = 0) where T : ITimerHandler
        {
            Unschedule(unscheduleId);
            long scheduleId = QuartzTimer.Delay<T>(ActorId, new DateTime(time) - DateTime.Now, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        public long Delay<T>(TimeSpan delay, Param param = null, long unscheduleId = 0) where T : ITimerHandler
        {
            Unschedule(unscheduleId);
            long scheduleId = QuartzTimer.Delay<T>(ActorId, delay, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        public long Schedule<T>(TimeSpan delay, TimeSpan interval, Param param = null, int repeatCount = -1, long unscheduleId = 0) where T : ITimerHandler
        {
            Unschedule(unscheduleId);
            long scheduleId = QuartzTimer.Schedule<T>(ActorId, delay, interval, param, repeatCount);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        public long Daily<T>(int hour = 0, int minute = 0, Param param = null, long unscheduleId = 0) where T : ITimerHandler
        {
            Unschedule(unscheduleId);
            long scheduleId = QuartzTimer.Daily<T>(ActorId, hour, minute, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        public long Weekly<T>(DayOfWeek dayOfWeek, int hour = 0, int minute = 0, Param param = null, long unscheduleId = 0) where T : ITimerHandler
        {
            Unschedule(unscheduleId);
            long scheduleId = QuartzTimer.Weekly<T>(ActorId, dayOfWeek, hour, minute, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        public long WithDayOfWeeks<T>(int hour, int minute, Param param, params DayOfWeek[] dayOfWeeks) where T : ITimerHandler
        {
            long scheduleId = QuartzTimer.WithDayOfWeeks<T>(ActorId, hour, minute, param, dayOfWeeks);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        public long Monthly<T>(int dayOfMonth, int hour = 0, int minute = 0, Param param = null, long unscheduleId = 0) where T : ITimerHandler
        {
            Unschedule(unscheduleId);
            long scheduleId = QuartzTimer.Monthly<T>(ActorId, dayOfMonth, hour, minute, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }

        public long WithCronExpression<T>(string cronExpression, Param param = null, long unscheduleId = 0) where T : ITimerHandler
        {
            Unschedule(unscheduleId);
            long scheduleId = QuartzTimer.WithCronExpression<T>(ActorId, cronExpression, param);
            ScheduleIdSet.Add(scheduleId);
            return scheduleId;
        }
    }
}
