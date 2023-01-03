using System.Collections.Concurrent;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Storage;
using Geek.Server.Core.Timer;
using Geek.Server.Core.Utils;
using MongoDB.Driver;
using NLog;

namespace Geek.Server.Core.Comps
{

    public sealed class StateComp
    {

        #region 仅DBModel.Mongodb调用
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly ConcurrentBag<Func<bool, bool, Task>> saveFuncs = new();

        public static void AddShutdownSaveFunc(Func<bool, bool, Task> shutdown)
        {
            saveFuncs.Add(shutdown);
        }

        /// <summary>
        /// 当游戏出现异常，导致无法正常回存，才需要将force=true
        /// 由后台http指令调度
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        public static async Task SaveAll(bool force = false)
        {
            try
            {
                var begin = DateTime.Now;
                var tasks = new List<Task>();
                foreach (var saveFunc in saveFuncs)
                {
                    tasks.Add(saveFunc(true, force));
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
                    await func(false, false);
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
        #endregion
    }

    public abstract class StateComp<TState> : BaseComp, IState where TState : CacheState, new()
    {

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        static readonly ConcurrentDictionary<long, TState> stateDic = new();

        public TState State { get; private set; }

        static StateComp()
        {
            if (Settings.DBModel == (int)DBModel.Mongodb)
                StateComp.AddShutdownSaveFunc(SaveAll);
        }

        public override async Task Active()
        {
            await base.Active();
            if (State != null)
                return;
            await ReadStateAsync();
        }

        public override Task Deactive()
        {
            if (Settings.DBModel == (int)DBModel.Mongodb)
                stateDic.TryRemove(ActorId, out _);
            return base.Deactive();
        }


        internal override bool ReadyToDeactive => State == null || !State.IsChanged().isChanged;

        internal override async Task SaveState()
        {
            try
            {
                await GameDB.SaveState(State);
            }
            catch (Exception e)
            {
                Log.Fatal($"StateComp.SaveState.Failed.StateId:{State.Id},{e}");
            }
        }

        public async Task ReadStateAsync()
        {
            State = await GameDB.LoadState<TState>(ActorId);
            if (Settings.DBModel == (int)DBModel.Mongodb)
            {
                stateDic.TryRemove(State.Id, out _);
                stateDic.TryAdd(State.Id, State);
            }
        }

        public Task WriteStateAsync()
        {
            return GameDB.SaveState(State);
        }


        #region 仅DBModel.Mongodb调用
        const int ONCE_SAVE_COUNT = 500;
        public static async Task SaveAll(bool shutdown, bool force = false)
        {
            static void AddReplaceModel(List<ReplaceOneModel<MongoState>> writeList, bool isChanged, long stateId, byte[] data)
            {
                if (isChanged)
                {
                    var mongoState = new MongoState()
                    {
                        Data = data,
                        Id = stateId.ToString(),
                        Timestamp = TimeUtils.CurrentTimeMillisUTC()
                    };
                    var filter = Builders<MongoState>.Filter.Eq(CacheState.UniqueId, mongoState.Id);
                    writeList.Add(new ReplaceOneModel<MongoState>(filter, mongoState) { IsUpsert = true });
                }
            }

            var writeList = new List<ReplaceOneModel<MongoState>>();
            var tasks = new List<Task<(bool, long, byte[])>>();
            foreach (var state in stateDic.Values)
            {
                var actor = ActorMgr.GetActor(state.Id);
                bool isChanged;
                byte[] data;
                if (actor != null)
                {
                    if (force)
                    {
                        (isChanged, data) = state.IsChanged();
                        AddReplaceModel(writeList, isChanged, state.Id, data);
                    }
                    else
                    {
                        tasks.Add(actor.SendAsync(() => state.IsChangedWithId()));
                    }
                }
            }

            var results = await Task.WhenAll(tasks);
            foreach (var (isChanged, stateId, data) in results)
            {
                AddReplaceModel(writeList, isChanged, stateId, data);
            }

            if (!writeList.IsNullOrEmpty())
            {
                var stateName = typeof(TState).FullName;
                StateComp.statisticsTool.Count(stateName, writeList.Count);
                Log.Debug($"状态回存 {stateName} count:{writeList.Count}");
                var db = GameDB.As<MongoDBConnection>().CurDB; 
                var col = db.GetCollection<MongoState>(stateName);
                for (int idx = 0; idx < writeList.Count; idx += ONCE_SAVE_COUNT)
                {
                    var list = writeList.GetRange(idx, Math.Min(ONCE_SAVE_COUNT, writeList.Count - idx));

                    bool save = false;
                    try
                    {
                        var result = await col.BulkWriteAsync(list, MongoDBConnection.BULK_WRITE_OPTIONS);
                        if (result.IsAcknowledged)
                        {
                            list.ForEach(model =>
                            {
                                if (stateDic.TryGetValue(long.Parse(model.Replacement.Id), out var st))
                                {
                                    st.AfterSaveToDB();
                                }
                            });
                            save = true;
                        }
                        else
                        {
                            Log.Error($"保存数据失败，类型:{typeof(TState).FullName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"保存数据异常，类型:{typeof(TState).FullName}，{ex}");
                    }
                    if (!save && shutdown)
                    {
                        await FileBackup.SaveToFile(list, stateName);
                    }
                }
            }
        }
        #endregion

    }
}
