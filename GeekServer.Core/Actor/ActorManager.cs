using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Geek.Server
{
    public static class ActorManager
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        readonly static ConcurrentDictionary<long, ComponentActor> actorMap = new ConcurrentDictionary<long, ComponentActor>();
        readonly static ConcurrentDictionary<long, DateTime> activeTimeMap = new ConcurrentDictionary<long, DateTime>();
        readonly static ConcurrentDictionary<long, WorkerActor> lifeActorDic = new ConcurrentDictionary<long, WorkerActor>();

        static WorkerActor GetLifeActor(long actorId)
        {
            lifeActorDic.TryGetValue(actorId, out var actor);
            lock (lifeActorDic)
            {
                lifeActorDic.TryGetValue(actorId, out actor);
                if (actor == null)
                {
                    actor = new WorkerActor();
                    lifeActorDic[actorId] = actor;
                }
            }
            return actor;
        }

        public static Task<T> Get<T>(long id) where T : ComponentActor
        {
            return GetLifeActor(id).SendAsync(() =>
            {
                //此接口允许actor返回空
                //所以不更新访问时间[回存和链接不需要更新访问时间]
                actorMap.TryGetValue(id, out var actor);
                if (!(actor is T))
                {
                    LOGGER.Error("actor 类型不匹配>get:{}, org:{} id:{}", typeof(T), actor == null ? "null" : actor.GetType().FullName, id);
#if DEBUG
                    throw new Exception("actor Get类型转换错误");
#endif
                }
                return (T)actor;
            });
        }

        public static Task ActorsForeach(Func<ComponentActor, Task> func)
        {
            foreach (var kv in actorMap)
            {
                var actor = kv.Value;
                func(actor);
            }
            return Task.CompletedTask;
        }

        public static async Task<T> GetOrNew<T>(long id) where T : IComponentActorAgent
        {
            return (T)await GetOrNew(typeof(T), id);
        }

        internal static async Task<IComponentActorAgent> GetOrNew(Type agentType, long id)
        {
            var now = DateTime.Now;
            float threshold = Settings.Ins.ActorRecycleTime * 0.67f;//0.67 ~= 2/3
            if (activeTimeMap.TryGetValue(id, out var activeTime)
                && (now - activeTime).TotalMinutes < threshold
                && actorMap.TryGetValue(id, out var actor))
            {
                activeTimeMap[id] = DateTime.Now;
                return actor.GetAgent(agentType);
            }

            return await GetLifeActor(id).SendAsync(() =>
            {
                actorMap.TryGetValue(id, out var actor);
                if (actor == null)
                {
                    var actorType = agentType.BaseType.GenericTypeArguments[0];
                    actor = (ComponentActor)Activator.CreateInstance(actorType);

                    actor.ActorId = id;
                    _ = actor.SendAsync(async () =>
                    {
                        await actor.Active();
                        await actor.GetAgent(agentType).Active();
                        await actor.InitListener();
                    }, false);
                    actorMap.TryAdd(id, actor);
                }
                activeTimeMap[id] = DateTime.Now;
                return actor.GetAgent(agentType);
            });
        }

        public static Task<bool> Remove(long id, bool checkActiveTime = true)
        {
            return GetLifeActor(id).SendAsync(() =>
            {
                if (!checkActiveTime
                || !activeTimeMap.ContainsKey(id)
                || (DateTime.Now - activeTimeMap[id]).TotalMinutes > Settings.Ins.ActorRecycleTime)
                {
                    if (actorMap.TryRemove(id, out _))
                    {
                        return activeTimeMap.TryRemove(id, out _);
                    }
                }
                return false;
            });
        }

        /// <summary>关服时调用</summary>
        public static async Task RemoveAll()
        {
            //可能是mongodb超连接数异常退出程序
            int count = 0;
            var taskList = new List<Task>();
            foreach (var kv in actorMap)
            {
                var actor = kv.Value;
                try
                {
                    var task = actor.SendAsync(actor.Deactive);
                    task = task.ContinueWith((o) => actor.SendAsync(actor.GetAgent().Deactive));
                    taskList.Add(task);
                    count++;
                }
                catch (Exception e)
                {
                    LOGGER.Error("actor deactive 异常:" + actor.ActorId + " " + actor.GetType());
                    LOGGER.Fatal(e.ToString());
                }
            }
            if (await Task.WhenAll(taskList).WaitAsync(TimeSpan.FromSeconds(30)))
                LOGGER.Error("remove all actors timeout");
            actorMap.Clear();
            LOGGER.Info("deactive all actors num=" + count);
            Console.WriteLine("deactive all actors num=" + count);
        }

        internal static Task CheckIdle()
        {
            foreach (var kv in actorMap)
            {
                var actorId = kv.Key;
                var actor = kv.Value;
                _ = actor.SendAsync(async () =>
                {
                    //Actor回收
                    if (!actor.AutoRecycleEnable)
                        return;

                    if ((DateTime.Now - activeTimeMap[actorId]).TotalMinutes > Settings.Ins.ActorRecycleTime)
                    {
                        await GetLifeActor(actorId).SendAsync(async () =>
                        {
                            if (activeTimeMap.ContainsKey(actorId)
                            && (DateTime.Now - activeTimeMap[actorId]).TotalMinutes > Settings.Ins.ActorRecycleTime)
                            {
                                if (await actor.ReadyToDeactive())
                                {
                                    actorMap.TryRemove(actorId, out var act);
                                    activeTimeMap.TryRemove(actorId, out _);
                                    await act.Deactive();
                                    await act.GetAgent().Deactive();
                                }
                            }
                        });
                    }
                    await actor.CheckIdle();
                });
            }
            return Task.CompletedTask;
        }
    }
}
