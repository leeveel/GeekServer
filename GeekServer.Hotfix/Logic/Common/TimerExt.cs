using System;
using System.Collections.Generic;

namespace Geek.Server
{
    public static class TimerExt
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>定时任务/每周</summary>
        public static long AddWeeklySchedule<TH>(this IComponentAgent agent, int hour, int minute, Param param = null, long unscheduleId = 0, params DayOfWeek[] days) where TH : ITimerHandler
        {
            if (unscheduleId > 0)
                agent.Unschedule(unscheduleId);
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal($"不能添加hotfix工程的类型作为Schedule参数 1  {typeof(TH)} {param.GetType()}");
                return -1;
            }
            if (Settings.Ins.IsDebug && !isListenerLegal<TH>(agent))
                return -1;

            var comp = (BaseComponent)agent.Owner;
            long id = QuartzTimer.AddWeeklySchedule(days, hour, minute, comp.ActorId, typeof(TH).FullName, param);
            return id;
        }

        /// <summary>定时任务/每月</summary>
        public static long AddMonthlySchedule<TH>(this IComponentAgent agent, int date, int hour, int minute, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
        {
            if (unscheduleId > 0)
                agent.Unschedule(unscheduleId);
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal($"不能添加hotfix工程的类型作为Schedule参数 2  {typeof(TH)} {param.GetType()}");
                return -1;
            }
            if (Settings.Ins.IsDebug && !isListenerLegal<TH>(agent))
                return -1;

            var comp = (BaseComponent)agent.Owner;
            long id = QuartzTimer.AddMonthlySchedule(date, hour, minute, comp.ActorId, typeof(TH).FullName, param);
            return id;
        }

        /// <summary>定时任务/一次性</summary>
        public static long AddOnceSchedule<TH>(this IComponentAgent agent, DateTime dateTime, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
        {
            if (unscheduleId > 0)
                agent.Unschedule(unscheduleId);
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal($"不能添加hotfix工程的类型作为Schedule参数 3  {typeof(TH)} {param.GetType()}");
                return -1;
            }
            if (Settings.Ins.IsDebug && !isListenerLegal<TH>(agent))
                return -1;

            var comp = (BaseComponent)agent.Owner;
            long id = QuartzTimer.AddOnceSchedule(dateTime, comp.ActorId, typeof(TH).FullName, param);
            return id;
        }

        /// <summary>定时任务/一次性</summary>
        public static long AddOnceSchedule<TH>(this IComponentAgent agent, long dateTimeTick, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
        {
            return agent.AddOnceSchedule<TH>(new DateTime(dateTimeTick), param, unscheduleId);
        }

        /// <summary>定时任务/每每天</summary>
        public static long AddDailySchedule<TH>(this IComponentAgent agent, int hour, int minute, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
        {
            if (unscheduleId > 0)
                agent.Unschedule(unscheduleId);
            if (HotfixMgr.IsFromHotfix(param))
            {
                LOGGER.Fatal($"不能添加hotfix工程的类型作为Schedule参数 4 {typeof(TH)} {param.GetType()}");
                return -1;
            }
            if (Settings.Ins.IsDebug && !isListenerLegal<TH>(agent))
                return -1;

            var comp = (BaseComponent)agent.Owner;
            long id = QuartzTimer.AddDailySchedule(hour, minute, comp.ActorId, typeof(TH).FullName, param);
            return id;
        }

        /// <summary>取消定时</summary>
        public static void Unschedule(this IComponentAgent agent, long id)
        {
            if (id <= 0)
                return;

            QuartzTimer.Remove(id);
        }

        /// <summary>取消定时</summary>
        public static void Unschedule(this IComponentAgent agent, IEnumerable<long> idList)
        {
            foreach (var id in idList)
                QuartzTimer.Remove(id);
        }

        static bool isListenerLegal<TH>(IComponentAgent agent) where TH : ITimerHandler
        {
            var comp = (BaseComponent)agent.Owner;
            ComponentActor actor = comp.Actor;

            var listenerType = typeof(TH);
            var agentType = listenerType.BaseType.GetGenericArguments()[0];
            //comp
            var compType = agentType.BaseType.GenericTypeArguments[0];
            var legal = ComponentMgr.Singleton.IsCompRegisted(actor, compType);
            if (!legal)
                LOGGER.Error($"TimerHandler类型错误，注册Timer的Actor未注册TimerHandler泛型参数类型Component，{actor.GetType()}未注册Comp:{compType}");
            return legal;
        }
    }
}