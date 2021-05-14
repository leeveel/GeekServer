using System;
using System.Collections.Generic;

namespace Geek.Server
{
    public static class TimerExt
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>定时任务/每周</summary>
        public static long AddWeeklySchedule<TH>(this IAgent agent, int hour, int minute, Param param = null, long unscheduleId = 0, params DayOfWeek[] days) where TH : ITimerHandler
        {
            if (unscheduleId > 0)
                agent.Unschedule(unscheduleId);
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal($"不能添加hotfix工程的类型作为Schedule参数 1  {typeof(TH)} {param.GetType()}");
                return -1;
            }
            getAgentInfo(agent, out var actorId, out var actorAgentType);
            long id = QuartzTimer.AddWeeklySchedule(days, hour, minute, actorId, actorAgentType, typeof(TH).FullName, param);
            return id;
        }

        /// <summary>定时任务/每月</summary>
        public static long AddMonthlySchedule<TH>(this IAgent agent, int date, int hour, int minute, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
        {
            if (unscheduleId > 0)
                agent.Unschedule(unscheduleId);
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal($"不能添加hotfix工程的类型作为Schedule参数 2  {typeof(TH)} {param.GetType()}");
                return -1;
            }
            getAgentInfo(agent, out var actorId, out var actorAgentType);
            long id = QuartzTimer.AddMonthlySchedule(date, hour, minute, actorId, actorAgentType, typeof(TH).FullName, param);
            return id;
        }

        /// <summary>定时任务/一次性</summary>
        public static long AddOnceSchedule<TH>(this IAgent agent, DateTime dateTime, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
        {
            if (unscheduleId > 0)
                agent.Unschedule(unscheduleId);
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal($"不能添加hotfix工程的类型作为Schedule参数 3  {typeof(TH)} {param.GetType()}");
                return -1;
            }
            getAgentInfo(agent, out var actorId, out var actorAgentType);
            long id = QuartzTimer.AddOnceSchedule(dateTime, actorId, actorAgentType, typeof(TH).FullName, param);
            return id;
        }

        /// <summary>定时任务/一次性</summary>
        public static long AddOnceSchedule<TH>(this IAgent agent, long dateTimeTick, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
        {
            return agent.AddOnceSchedule<TH>(new DateTime(dateTimeTick), param, unscheduleId);
        }

        /// <summary>定时任务/每每天</summary>
        public static long AddDailySchedule<TH>(this IAgent agent, int hour, int minute, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
        {
            if (unscheduleId > 0)
                agent.Unschedule(unscheduleId);
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal($"不能添加hotfix工程的类型作为Schedule参数 4 {typeof(TH)} {param.GetType()}");
                return -1;
            }
            getAgentInfo(agent, out var actorId, out var actorAgentType);
            long id = QuartzTimer.AddDailySchedule(hour, minute, actorId, actorAgentType, typeof(TH).FullName, param);
            return id;
        }

        /// <summary>取消定时</summary>
        public static void Unschedule(this IAgent agent, long id)
        {
            if (id <= 0)
                return;

            QuartzTimer.Remove(id);
        }

        /// <summary>取消定时</summary>
        public static void Unschedule(this IAgent agent, IEnumerable<long> idList)
        {
            foreach (var id in idList)
                QuartzTimer.Remove(id);
        }

        static void getAgentInfo(IAgent agent, out long actorId, out string actorAgentType)
        {
            actorId = 0;
            actorAgentType = null;
            if (agent is IComponentAgent)
            {
                var comp = (BaseComponent)agent.Owner;
                actorId = comp.ActorId;
                actorAgentType = comp.Actor.AgentTypeName;
            }
            else if (agent is IComponentActorAgent)
            {
                var actor = (ComponentActor)agent.Owner;
                actorId = actor.ActorId;
                actorAgentType = actor.AgentTypeName;
            }
        }
    }
}