using System.Collections.Concurrent;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Storage;
using Geek.Server.Core.Timer;
using Geek.Server.Core.Utils;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NLog;

namespace Geek.Server.Core.Comps
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
            State = await MongoDBConnection.LoadState<TState>(ActorId);
            stateDic.TryRemove(State.Id, out _);
            stateDic.TryAdd(State.Id, State);
        }

        public Task WriteStateAsync()
        {
            return MongoDBConnection.SaveState(State);
        }

        const int ONCE_SAVE_COUNT = 500;

        public static async Task SaveAll(bool shutdown)
        {
            var writeList = new List<ReplaceOneModel<TState>>();
            var tasks = new List<Task<(bool, byte[])>>();
            foreach (var state in stateDic.Values)
            {
                var actor = ActorMgr.GetActor(state.Id);
                bool isChanged;
                byte[] data;
                if (actor != null)
                {
                    tasks.Add(actor.SendAsync(() => state.IsChanged()));
                }
                else
                {
                    (isChanged, data) = state.IsChanged();
                    CheckChangeAndAddReplaceModel(writeList, isChanged, data);
                }
            }

            var results = await Task.WhenAll(tasks);
            foreach (var (isChanged, data) in results)
            {
                CheckChangeAndAddReplaceModel(writeList, isChanged, data);
            }

            if (!writeList.IsNullOrEmpty())
            {
                var stateName = typeof(TState).FullName;
                StateComp.statisticsTool.Count(stateName, writeList.Count);
                Log.Debug($"状态回存 {stateName} count:{writeList.Count}");
                var db = MongoDBConnection.CurDB;
                var col = db.GetCollection<TState>();
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
                                if (stateDic.TryGetValue(model.Replacement.Id, out var st))
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
                        await FileBackup.SaveToFile(list);
                    }
                }
            }

            static void CheckChangeAndAddReplaceModel(List<ReplaceOneModel<TState>> writeList, bool isChanged, byte[] data)
            {
                if (isChanged)
                {
                    var _state = BsonSerializer.Deserialize<TState>(data);
                    var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, _state.Id);
                    writeList.Add(new ReplaceOneModel<TState>(filter, _state) { IsUpsert = true });
                }
            }
        }
    }
}
