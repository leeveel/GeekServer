using NLog;
using System.Collections.Concurrent;

namespace Geek.Server
{

    public sealed class StateComp
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly ConcurrentBag<Func<bool, Task>> saveFuncs = new();

        public static void AddShutdownSaveFunc(Func<bool, Task> shutdown)
        {
            saveFuncs.Add(shutdown);
        }

        public static async Task SaveAll()
        {
            try
            {
                var begin = DateTime.Now;
                var tasks = new List<Task>();
                foreach (var saveFunc in saveFuncs)
                {
                    tasks.Add(saveFunc(true));
                }
                await Task.WhenAll(tasks);
                Log.Info($"save all state, use: {(DateTime.Now - begin).TotalMilliseconds}ms");
            }
            catch (Exception e)
            {
                Log.Error($"save all state error \n{e}");
            }
        }

        /// <summary>
        /// 定时回存所有数据
        /// </summary>
        public static async Task TimerSave()
        {
            try
            {
                foreach (var func in saveFuncs)
                {
                    await func(false);
                    if (!GlobalTimer.working)
                        return;
                }
            }
            catch (Exception e)
            {
                Log.Info("timer save state error");
                Log.Error(e.ToString());
            }
        }

        public static readonly StatisticsTool statisticsTool = new();
    }

    public abstract class StateComp<TState> : BaseComp, IState where TState : CacheState, new()
    {

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        static readonly ConcurrentDictionary<long, TState> stateDic = new();

        static StateComp()
        {
            StateComp.AddShutdownSaveFunc(SaveAll);
        }

        public TState State { get; private set; }

        public override bool IsActive => State != null;

        public override Task Active()
        {
            if (State != null)
                return Task.CompletedTask;
            return ReadStateAsync();
        }

        public override Task Deactive()
        {
            stateDic.TryRemove(ActorId, out _);
            return base.Deactive();
        }

        internal override bool ReadyToDeactive
        {
            get 
            {
                RocksDBConnection.Singleton.SaveState(State);
                return true;
            }
        }

        public Task ReadStateAsync()
        {
            State = RocksDBConnection.Singleton.LoadState<TState>(ActorId);
            stateDic.TryRemove(State.Id, out _);
            stateDic.TryAdd(State.Id, State);
            return Task.CompletedTask;
        }

        public Task WriteStateAsync()
        {
            RocksDBConnection.Singleton.SaveState(State);
            return Task.CompletedTask;
        }

        const int ONCE_SAVE_COUNT = 500;
        public static async Task SaveAll(bool shutdown)
        {
            var taskList = new List<Task>();
            foreach (var state in stateDic.Values)
            {
                var actor = ActorMgr.GetActor(state.Id);
                if (actor != null)
                {
                    taskList.Add(actor.SendAsync(() => { RocksDBConnection.Singleton.SaveState(state); }, int.MaxValue));
                }
                else
                {
                    RocksDBConnection.Singleton.SaveState(state);
                }
            }
            await Task.WhenAll(taskList.ToArray()); 
        }

    }
}
