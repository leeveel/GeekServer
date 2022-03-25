using System;
using System.Collections.Generic;

namespace Geek.Server
{
    public static class TimerExt
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        static WorkerActor roleIDActor = new WorkerActor();
        static Dictionary<long, List<long>> roleTimerMap = new Dictionary<long, List<long>>();
        static void addRoleTimer(long roleId, long timerId)
        {
            roleIDActor.SendAsync(() => {
                roleTimerMap.TryGetValue(roleId, out var idList);
                if (idList == null)
                {
                    idList = new List<long>();
                    roleTimerMap[roleId] = idList;
                }
                idList.Add(timerId);
            });
        }

        public static void RemoveRoleTimers(this IComponentAgent agent)
        {
            roleIDActor.SendAsync(() =>
            {
                roleTimerMap.TryGetValue(agent.EntityId, out var idList);
                foreach (var id in idList)
                    QuartzTimer.Remove(id);

            });
        }

        /// <summary>定时任务/每周</summary>
        public static long Schedule<TH>(this IComponentAgent agent, int hour, int minute, Param param = null, long unscheduleId = 0, params DayOfWeek[] days) where TH : ITimerHandler
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

            getAgentInfo(agent, out var entityId, out var entityType);
            long id = QuartzTimer.AddWeeklySchedule(days, hour, minute, entityId, typeof(TH).FullName, param);
            if (entityType == EntityType.Role)
                addRoleTimer(entityId, id);
            return id;
        }

        /// <summary>定时任务/每月</summary>
        public static long Schedule<TH>(this IComponentAgent agent, int date, int hour, int minute, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
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

            getAgentInfo(agent, out var entityId, out var entityType);
            long id = QuartzTimer.AddMonthlySchedule(date, hour, minute, entityId, typeof(TH).FullName, param);
            if (entityType == EntityType.Role)
                addRoleTimer(entityId, id);
            return id;
        }

        /// <summary>定时任务/一次性</summary>
        public static long Schedule<TH>(this IComponentAgent agent, DateTime dateTime, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
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

            getAgentInfo(agent, out var entityId, out var entityType);
            long id = QuartzTimer.AddOnceSchedule(dateTime, entityId, typeof(TH).FullName, param);
            if (entityType == EntityType.Role)
                addRoleTimer(entityId, id);
            return id;
        }

        /// <summary>定时任务/一次性</summary>
        public static long Schedule<TH>(this IComponentAgent agent, long dateTimeTick, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
        {
            return agent.Schedule<TH>(new DateTime(dateTimeTick), param, unscheduleId);
        }

        /// <summary>定时任务/每每天</summary>
        public static long Schedule<TH>(this IComponentAgent agent, int hour, int minute, Param param = null, long unscheduleId = 0) where TH : ITimerHandler
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

            getAgentInfo(agent, out var entityId, out var entityType);
            long id = QuartzTimer.AddDailySchedule(hour, minute, entityId, typeof(TH).FullName, param);
            if (entityType == EntityType.Role)
                addRoleTimer(entityId, id);
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

        static void getAgentInfo(IComponentAgent agent, out long actorId, out EntityType entityType)
        {
            actorId = 0;
            entityType = EntityType.None;
            if (agent is IComponentAgent)
            {
                var comp = agent.Owner;
                actorId = comp.EntityId;
                entityType = (EntityType)comp.EntityType;
            }
        }

        static bool isListenerLegal<TH>(IComponentAgent agent) where TH : ITimerHandler
        {
            EntityType entityType = EntityType.None;
            if (agent is IComponentAgent)
            {
                var comp = agent.Owner;
                entityType = (EntityType)comp.EntityType;
            }

            var listenerType = typeof(TH);
            var agentType = listenerType.BaseType.GetGenericArguments()[0];
            if (agentType.GetInterface(typeof(IComponentAgent).FullName) != null)
            {
                //comp
                var compType = agentType.BaseType.GenericTypeArguments[0];
                var legal = CompSetting.Singleton.IsCompRegisted((int)entityType, compType);
                if (!legal)
                    LOGGER.Error($"TimerHandler类型错误，注册Timer的Actor未注册TimerHandler泛型参数类型Component，{entityType}未注册Comp:{compType}   {typeof(TH)}");
                return legal;
            }
            LOGGER.Error("TimerHandler类型错误");
            return false;
        }
    }
}

