using Geek.Server.Core.Actors;
using Geek.Server.Core.Storage;
using Geek.Server.Core.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using System.Collections.Concurrent;

namespace Geek.Server.Core.Comps
{

    public sealed class StateComp
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly ConcurrentBag<Func<bool, bool, Task>> saveFuncs = new();

        public static void AddSaveFunc(Func<bool, bool, Task> func)
        {
            saveFuncs.Add(func);
        }

        /// <summary>
        /// 当游戏出现异常，导致无法正常回存，才需要将force=true 
        /// </summary>  
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

        public static readonly StatisticsTool statisticsTool = new();
    }

    public abstract class StateComp<TState> : BaseComp, IState where TState : CacheState, new()
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        static readonly ConcurrentDictionary<long, TState> stateDic = new();

        public TState State { get; private set; }

        static StateComp()
        {
            StateComp.AddSaveFunc(SaveStates);
        }

        public override async Task Active()
        {
            await base.Active();
            if (State != null)
                return;
            await ReadState();
        }

        public override Task Deactive()
        {
            stateDic.TryRemove(ActorId, out _);
            return base.Deactive();
        }


        internal override bool ReadyToDeactive => State == null || !State.IsChanged();

        public async Task ReadState()
        {
            State = await GameDB.LoadState<TState>(ActorId);
            stateDic.TryRemove(State.Id, out _);
            stateDic.TryAdd(State.Id, State);
        }


        const int ONCE_SAVE_COUNT = 300; 

        static async Task MongoDBSaveStates(bool shutdown, bool force = false)
        {
            var idList = new List<long>();
            var writeList = new List<ReplaceOneModel<MongoDBDocument>>();
            if (shutdown)
            {
                foreach (var state in stateDic.Values)
                {
                    if (state.IsChanged())
                    {
                        var bsonDoc = state.ToBsonDocument();
                        lock (writeList)
                        {
                            var filter = Builders<MongoDBDocument>.Filter.Eq("_id", state.Id);
                            writeList.Add(new ReplaceOneModel<MongoDBDocument>(filter, bsonDoc) { IsUpsert = true });
                            idList.Add(state.Id);
                        }
                    }
                }
            }
            else
            {
                var tasks = new List<Task>();

                foreach (var state in stateDic.Values)
                {
                    var actor = ActorMgr.GetActor(state.Id);
                    if (actor != null)
                    {
                        tasks.Add(actor.SendAsync(() =>
                        {
                            if (!force && !state.IsChanged())
                                return;
                            var bsonDoc = state.ToBsonDocument();
                            lock (writeList)
                            {
                                var filter = Builders<MongoDBDocument>.Filter.Eq("_id", state.Id);
                                writeList.Add(new ReplaceOneModel<MongoDBDocument>(filter, bsonDoc) { IsUpsert = true });
                                idList.Add(state.Id);
                            }
                        }));
                    }
                }

                await Task.WhenAll(tasks);
            }


            if (!writeList.IsNullOrEmpty())
            {
                var stateName = typeof(TState).FullName;
                StateComp.statisticsTool.Count(stateName, writeList.Count);
                Log.Debug($"状态回存 {stateName} count:{writeList.Count}");
                var db = GameDB.As<MongoDBConnection>().CurDB;
                var col = db.GetCollection<MongoDBDocument>(stateName);
                for (int idx = 0; idx < writeList.Count; idx += ONCE_SAVE_COUNT)
                {
                    var docs = writeList.GetRange(idx, Math.Min(ONCE_SAVE_COUNT, writeList.Count - idx));
                    var ids = idList.GetRange(idx, docs.Count);

                    bool save = false;
                    try
                    {
                        var result = await col.BulkWriteAsync(docs, MongoDBConnection.BULK_WRITE_OPTIONS);
                        if (result.IsAcknowledged)
                        {
                            foreach (var id in ids)
                            {
                                stateDic.TryGetValue(id, out var state);
                                if (state == null)
                                    continue;
                                state.AfterSaveToDB();
                            }
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
                        await FileBackup.SaveToFile(ids, docs, stateName);
                    }
                }
            }
        }

        public static async Task SaveStates(bool shutdown, bool force = false)
        {
            await MongoDBSaveStates(shutdown, force);
        }
    }
}
