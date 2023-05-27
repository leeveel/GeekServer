using System.Collections.Concurrent;
using Geek.Server.Core.Actors.Impl;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Timer;

namespace Geek.Server.Core.Actors
{
    sealed public class Actor
    {

        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<Type, BaseComp> compDic = new();

        public long Id { get; init; }

        public ActorType Type { get; init; }

        public WorkerActor WorkerActor { get; init; }

        public bool AutoRecycle { get; private set; } = false;

        public HashSet<long> ScheduleIdSet = new();

        public void SetAutoRecycle(bool autoRecycle)
        {
            Tell(() =>
            {
                AutoRecycle = autoRecycle;
            });
        }

        public async Task<T> GetCompAgent<T>() where T : ICompAgent
        {
            return (T)await GetCompAgent(typeof(T));
        }

        public async Task<ICompAgent> GetCompAgent(Type agentType)
        {
            var compType = agentType.BaseType.GetGenericArguments()[0];
            var comp = compDic.GetOrAdd(compType, k => CompRegister.NewComp(this, k));
            var agent = comp.GetAgent(agentType);
            if (!comp.IsActive)
            {
                await SendAsyncWithoutCheck(async () =>
                {
                    await comp.Active();
                    agent.Active();
                });
            }
            return agent;
        }

        public const int TIME_OUT = int.MaxValue;

        public Actor(long id, ActorType type)
        {
            Id = id;
            Type = type;
            WorkerActor = new(id);

            if (type == ActorType.Role)
            {
                Tell(() => SetAutoRecycle(true));
            }
            else
            {
                Tell(() => CompRegister.ActiveComps(this));
            }
        }

        public async Task CrossDay(int openServerDay)
        {
            Log.Debug($"actor跨天 id:{Id} type:{Type}");
            foreach (var comp in compDic.Values)
            {
                var agent = comp.GetAgent();
                if (agent is ICrossDay crossDay)
                {
                    // 使用try-catch缩小异常影响范围
                    try
                    {
                        await crossDay.OnCrossDay(openServerDay);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{agent.GetType().FullName}跨天失败 actorId:{Id} actorType:{Type} 异常：\n{e}");
                    }
                }
            }
        }

        internal bool ReadyToDeactive => compDic.Values.All(item => item.ReadyToDeactive);

        internal async Task SaveAllState()
        {
            foreach (var item in compDic)
            {
               await item.Value.SaveState();
            }
        }

        public async Task Deactive()
        {
            foreach (var item in compDic.Values)
            {
                await item.Deactive();
            }
        }

        #region actor 入队
        public void Tell(Action work, int timeout = TIME_OUT)
        {
            WorkerActor.Tell(work, timeout);
        }

        public void Tell(Func<Task> work, int timeout = TIME_OUT)
        {
            WorkerActor.Tell(work, timeout);
        }

        public Task SendAsync(Action work, int timeout = TIME_OUT)
        {
            return WorkerActor.SendAsync(work, timeout);
        }

        public Task<T> SendAsync<T>(Func<T> work, int timeout = TIME_OUT)
        {
            return WorkerActor.SendAsync(work, timeout);
        }

        public Task SendAsync(Func<Task> work, int timeout = TIME_OUT)
        {
            return WorkerActor.SendAsync(work, timeout);
        }

        public Task SendAsyncWithoutCheck(Func<Task> work, int timeout = TIME_OUT)
        {
            return WorkerActor.SendAsync(work, timeout, false);
        }

        public Task<T> SendAsync<T>(Func<Task<T>> work, int timeout = TIME_OUT)
        {
            return WorkerActor.SendAsync(work, timeout);
        }
        #endregion

        public override string ToString()
        {
            return $"{base.ToString()}_{Type}_{Id}";
        }

        public void ClearAgent()
        {
            foreach (var comp in compDic.Values)
            {
                comp.ClearCacheAgent();
            }
        }
    }
}
