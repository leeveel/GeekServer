using MongoDB.Bson.Serialization;
using MongoDB.Driver;
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

        internal override bool ReadyToDeactive => State == null || !State.IsChanged().isChanged;

        public async Task ReadStateAsync()
        {
            State = await RocksDBConnection.Singleton.LoadState<TState>(ActorId);
            stateDic.TryRemove(State.Id, out _);
            stateDic.TryAdd(State.Id, State);
        }

        public async Task WriteStateAsync()
        {
            await RocksDBConnection.Singleton.SaveState(State);
        }

        const int ONCE_SAVE_COUNT = 500;
        public static async Task SaveAll(bool shutdown)
        {
            var changeDataId = new List<long>();
            var changeData = new List<byte[]>();
            var tasks = new List<Task<(bool, long, byte[])>>();
            foreach (var state in stateDic.Values)
            {
                var actor = ActorMgr.GetActor(state.Id);
                if (actor != null)
                {
                    tasks.Add(actor.SendAsync(() => state.IsChangedWithStateId()));
                }
                else
                {
                    var (isChanged, stateId, data) = state.IsChangedWithStateId();
                    if (isChanged)
                        changeData.Add(data);    
                }
            }

            var results = await Task.WhenAll(tasks);
            foreach (var (isChanged, stateId, data) in results)
            {
                if (isChanged)
                {
                    changeDataId.Add(stateId);
                    changeData.Add(data);
                }
            }

            if (!changeData.IsNullOrEmpty())
            {
                var stateName = typeof(TState).FullName;
                StateComp.statisticsTool.Count(stateName, changeData.Count);
                Log.Debug($"状态回存 {stateName} count:{changeData.Count}");
                var db = RocksDBConnection.Singleton.CurDataBase;
                var dbTable = db.GetTable<TState>();
                for (int idx = 0; idx < changeData.Count; idx += ONCE_SAVE_COUNT)
                {
                    var ids = changeDataId.GetRange(idx, Math.Min(ONCE_SAVE_COUNT, changeDataId.Count - idx));
                    var datas = changeData.GetRange(idx, Math.Min(ONCE_SAVE_COUNT, changeData.Count - idx));
                    try
                    {
                        dbTable.SetRawBatch(ids, datas);
                        foreach (var id in ids)
                        {
                            stateDic.TryGetValue(id, out var state);
                            if (state == null)
                                continue;
                            state.AfterSaveToDB();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"保存数据异常，类型:{typeof(TState).FullName}，{ex}");
                    }
                }
            }
        
        }
    }
}
