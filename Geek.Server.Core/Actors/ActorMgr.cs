using System.Collections.Concurrent;
using Geek.Server.Core.Actors.Impl;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Timer;
using Geek.Server.Core.Utils;

namespace Geek.Server.Core.Actors
{
    public class ActorMgr
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private static readonly ConcurrentDictionary<long, Actor> actorDic = new();

        public static async Task<T> GetCompAgent<T>(long actorId) where T : ICompAgent
        {
            var actor = await GetOrNew(actorId);
            return await actor.GetCompAgent<T>();
        }

        public static bool HasActor(long id)
        {
            return actorDic.ContainsKey(id);
        }

        internal static Actor GetActor(long actorId)
        {
            actorDic.TryGetValue(actorId, out var actor);
            return actor;
        }

        internal static async Task<ICompAgent> GetCompAgent(long actorId, Type agentType)
        {
            var actor = await GetOrNew(actorId);
            return await actor.GetCompAgent(agentType);
        }

        public static Task<T> GetCompAgent<T>() where T : ICompAgent
        {
            var compType = HotfixMgr.GetCompType(typeof(T));
            var actorType = CompRegister.GetActorType(compType);
            return GetCompAgent<T>(IdGenerator.GetActorID(actorType));
        }

        internal static async Task<Actor> GetOrNew(long actorId)
        {
            var actorType = IdGenerator.GetActorType(actorId);
            if (actorType == ActorType.Role)
            {
                var now = DateTime.Now;
                if (activeTimeDic.TryGetValue(actorId, out var activeTime)
                    && (now - activeTime).TotalMinutes < 10
                    && actorDic.TryGetValue(actorId, out var actor))
                {
                    activeTimeDic[actorId] = now;
                    return actor;
                }
                else
                {
                    return await GetLifeActor(actorId).SendAsync(() =>
                    {
                        activeTimeDic[actorId] = now;
                        return actorDic.GetOrAdd(actorId, k => new Actor(k, IdGenerator.GetActorType(k)));
                    });
                }
            }
            else
            {
                return actorDic.GetOrAdd(actorId, k => new Actor(k, IdGenerator.GetActorType(k)));
            }
        }

        public static Task AllFinish()
        {
            var tasks = new List<Task>();
            foreach (var actor in actorDic.Values)
            {
                tasks.Add(actor.SendAsync(() => true));
            }
            return Task.WhenAll(tasks);
        }

        private static readonly ConcurrentDictionary<long, DateTime> activeTimeDic = new();

        private static readonly List<WorkerActor> workerActors = new();
        private const int workerCount = 10;
        static ActorMgr()
        {
            for (int i = 0; i < workerCount; i++)
            {
                workerActors.Add(new WorkerActor());
            }
        }

        private static WorkerActor GetLifeActor(long actorId)
        {
            return workerActors[(int)(actorId % workerCount)];
        }

        /// <summary>
        /// 目前只回收玩家
        /// </summary>
        public static Task CheckIdle()
        {
            foreach (var actor in actorDic.Values)
            {
                if (actor.AutoRecycle)
                {
                    actor.Tell(async () =>
                    {
                        if (actor.AutoRecycle
                        && (DateTime.Now - activeTimeDic[actor.Id]).TotalMinutes > 15)
                        {
                            await GetLifeActor(actor.Id).SendAsync(async () =>
                            {
                                if (activeTimeDic.TryGetValue(actor.Id, out var activeTime)
                                && (DateTime.Now - activeTimeDic[actor.Id]).TotalMinutes > 15)
                                {
                                    // 防止定时回存失败时State被直接移除
                                    if (actor.ReadyToDeactive)
                                    {
                                        await actor.Deactive();
                                        actorDic.TryRemove(actor.Id, out var _);
                                        Log.Debug($"actor回收 id:{actor.Id} type:{actor.Type}");
                                    }
                                    else
                                    {
                                        // 不能存就久一点再判断
                                        activeTimeDic[actor.Id] = DateTime.Now;
                                    }
                                }
                                return true;
                            });
                        }
                    });
                }
            }
            return Task.CompletedTask;
        }


        public static async Task SaveAll()
        {
            try
            {
                var begin = DateTime.Now;
                var taskList = new List<Task>();
                foreach (var actor in actorDic.Values)
                {
                    taskList.Add(actor.SendAsync(async () => await actor.SaveAllState()));
                }
                await Task.WhenAll(taskList);
                Log.Info($"save all state, use: {(DateTime.Now - begin).TotalMilliseconds}ms");
            }
            catch (Exception e)
            {
                Log.Error($"save all state error \n{e}"); throw;
            }
        }

        //public static readonly StatisticsTool statisticsTool = new();
        const int ONCE_SAVE_COUNT = 1000;
        /// <summary>
        ///  定时回存所有数据
        /// </summary>
        /// <returns></returns>
        public static async Task TimerSave()
        {
            try
            {
                int count = 0;
                var taskList = new List<Task>();
                foreach (var actor in actorDic.Values)
                {
                    //如果定时回存的过程中关服了，直接终止定时回存，因为关服时会调用SaveAll以保证数据回存
                    if (!GlobalTimer.working)
                        return;
                    if (count < ONCE_SAVE_COUNT)
                    {
                        taskList.Add(actor.SendAsync(async () => await actor.SaveAllState()));
                        count++;
                    }
                    else
                    {
                        await Task.WhenAll(taskList);
                        await Task.Delay(1000);
                        taskList.Clear();
                        count = 0;
                    }
                }
                if (taskList.Count > 0)
                {
                    await Task.WhenAll(taskList);
                }
            }
            catch (Exception e)
            {
                Log.Info("timer save state error");
                Log.Error(e.ToString());
            }
        }


        public static Task RoleCrossDay(int openServerDay)
        {
            foreach (var actor in actorDic.Values)
            {
                if (actor.Type == ActorType.Role)
                {
                    actor.Tell(() => actor.CrossDay(openServerDay));
                }
            }
            return Task.CompletedTask;
        }

        const int CROSS_DAY_GLOBAL_WAIT_SECONDS = 60;
        const int CROSS_DAY_NOT_ROLE_WAIT_SECONDS = 120;

        public static async Task CrossDay(int openServerDay, ActorType driverActorType)
        {
            // 驱动actor优先执行跨天
            var id = IdGenerator.GetActorID(driverActorType);
            var driverActor = actorDic[id];
            await driverActor.CrossDay(openServerDay);

            var begin = DateTime.Now;
            int a = 0;
            int b = 0;
            foreach (var actor in actorDic.Values)
            {
                if (actor.Type > ActorType.Separator && actor.Type != driverActorType)
                {
                    b++;
                    actor.Tell(async () =>
                    {
                        Log.Info($"全局Actor：{actor.Type}执行跨天");
                        await actor.CrossDay(openServerDay);
                        Interlocked.Increment(ref a);
                    });
                }
            }
            while (a < b)
            {
                if ((DateTime.Now - begin).TotalSeconds > CROSS_DAY_GLOBAL_WAIT_SECONDS)
                {
                    Log.Warn($"全局comp跨天耗时过久，不阻止其他comp跨天，当前已过{CROSS_DAY_GLOBAL_WAIT_SECONDS}秒");
                    break;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
            var globalCost = (DateTime.Now - begin).TotalMilliseconds;
            Log.Info($"全局comp跨天完成 耗时：{globalCost:f4}ms");
            a = 0;
            b = 0;
            foreach (var actor in actorDic.Values)
            {
                if (actor.Type < ActorType.Separator && actor.Type != ActorType.Role)
                {
                    b++;
                    actor.Tell(async () =>
                    {
                        await actor.CrossDay(openServerDay);
                        Interlocked.Increment(ref a);
                    });
                }
            }
            while (a < b)
            {
                if ((DateTime.Now - begin).TotalSeconds > CROSS_DAY_NOT_ROLE_WAIT_SECONDS)
                {
                    Log.Warn($"非玩家comp跨天耗时过久，不阻止玩家comp跨天，当前已过{CROSS_DAY_NOT_ROLE_WAIT_SECONDS}秒");
                    break;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
            var otherCost = (DateTime.Now - begin).TotalMilliseconds - globalCost;
            Log.Info($"非玩家comp跨天完成 耗时：{otherCost:f4}ms");
        }

        public static async Task RemoveAll()
        {
            var tasks = new List<Task>();
            foreach (var actor in actorDic.Values)
            {
                tasks.Add(actor.Deactive());
            }
            await Task.WhenAll(tasks);
        }

        public static Task Remove(long actorId)
        {
            if (actorDic.Remove(actorId, out var actor))
            {
                actor.Tell(actor.Deactive);
            }
            return Task.CompletedTask;
        }

        public static void ActorForEach<T>(Func<T, Task> func) where T : ICompAgent
        {
            var agentType = typeof(T);
            var compType = HotfixMgr.GetCompType(agentType);
            var actorType = CompRegister.GetActorType(compType);
            foreach (var actor in actorDic.Values)
            {
                if (actor.Type == actorType)
                {
                    actor.Tell(async () =>
                    {
                        var comp = await actor.GetCompAgent<T>();
                        await func(comp);
                    });
                }
            }
        }

        public static void ActorForEach<T>(Action<T> action) where T : ICompAgent
        {
            var agentType = typeof(T);
            var compType = HotfixMgr.GetCompType(agentType);
            var actorType = CompRegister.GetActorType(compType);
            foreach (var actor in actorDic.Values)
            {
                if (actor.Type == actorType)
                {
                    actor.Tell(async () =>
                    {
                        var comp = await actor.GetCompAgent<T>();
                        action(comp);
                    });
                }
            }
        }

        public static void ClearAgent()
        {
            foreach (var actor in actorDic.Values)
            {
                actor.Tell(actor.ClearAgent);
            }
        }
    }
}