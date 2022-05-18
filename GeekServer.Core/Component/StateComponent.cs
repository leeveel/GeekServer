using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;


namespace Geek.Server
{
    public sealed class StateComponent
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        static readonly object lockObj = new object();
        static readonly ConcurrentQueue<Func<Task>> shutdownFuncList = new ConcurrentQueue<Func<Task>>();
        static readonly ConcurrentQueue<Func<Task>> timerFuncList = new ConcurrentQueue<Func<Task>>();

        public static void AddShutdownSaveFunc(Func<Task> shutdown, Func<Task> timer)
        {
            lock (lockObj)
            {
                if (!shutdownFuncList.Contains(shutdown))
                    shutdownFuncList.Enqueue(shutdown);
                if (!timerFuncList.Contains(timer))
                    timerFuncList.Enqueue(timer);
            }
        }

        /// <summary>
        /// 停服时回存所有数据
        /// </summary>
        public static async Task SaveAllState()
        {
            try
            {
                var start = DateTime.Now;
                var taskList = new List<Task>();
                foreach(var func in shutdownFuncList)
                    taskList.Add(func());
                await Task.WhenAll(taskList);
                LOGGER.Info("all state save time:{}毫秒", (DateTime.Now - start).TotalMilliseconds);
            }
            catch (Exception e)
            {
                LOGGER.Info("save all state error");
                LOGGER.Error(e.ToString());
            }
        }

        /// <summary>
        /// 定时回存所有数据
        /// </summary>
        public static async Task TimerSave()
        {
            try
            {
                foreach (var func in timerFuncList)
                {
                    await func();
                    if (!GlobalDBTimer.Singleton.Working)
                        return;
                }
            }
            catch (Exception e)
            {
                LOGGER.Info("timer save state error");
                LOGGER.Error(e.ToString());
            }
        }
    }


    /// <summary>
    /// 所有需要存储数据的系统由此派生
    /// </summary>
    public abstract class StateComponent<TState> : BaseComponent, IState where TState : DBState, new()
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        protected TState _State;
        public DBState State => _State;

        public override async Task Active()
        {
            if (IsActive)
                return;

            await ReadStateAsync();

            StateComponent.AddShutdownSaveFunc(saveAllStateOfAType, timerSaveAllStateOfAType);
            aTypeAllStateMap.TryRemove(_State.Id, out _);
            aTypeAllStateMap.TryAdd(_State.Id, _State);
            stateCompType = GetType();
        }

        static Type stateCompType;
        static readonly ConcurrentDictionary<long, TState> aTypeAllStateMap = new ConcurrentDictionary<long, TState>();
        static async Task saveAllStateOfAType()
        {
            //批量回存当前类型的所有state
            var batchList = new List<ReplaceOneModel<TState>>();
            foreach (var kv in aTypeAllStateMap)
            {
                var state = kv.Value;
                var entity = EntityMgr.GetEntity(state.Id);
                if (entity == null || entity.ReadOnly)
                    continue;

                //关服时直接调用，actor逻辑已停，安全
                if (!state.IsChangedComparedToDB())
                    continue;

                state.ReadyToSaveToDB();
                var filter = Builders<TState>.Filter.Eq(MongoField.Id, state.Id);
                var saveModel = new ReplaceOneModel<TState>(filter, state) { IsUpsert = true };
                batchList.Add(saveModel);
            }

            var db = MongoDBConnection.Singleton.CurDateBase;
            var col = db.GetCollection<TState>(BaseDBState.WrapperFullName<TState>());
            int idx = 0;
            int once = 500;
            while (idx < batchList.Count)
            {
                var list = batchList.GetRange(idx, Math.Min(once, batchList.Count - idx));
                idx += once;
                bool saved = false;
                try
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        var result = await col.BulkWriteAsync(list, new BulkWriteOptions() { IsOrdered = false });
                        if (result.IsAcknowledged)
                        {
                            saved = true;
                            break;
                        }
                        await Task.Delay(1000);
                    }
                    await Task.Delay(10);
                }
                catch (Exception e)
                {
                    LOGGER.Error(e.ToString());
                }

                if (!saved)
                {
                    LOGGER.Error("存数据库失败，可以先存到磁盘");
                    FileBackUp.StoreToFile(list);
                    await ExceptionMonitor.Send($"【{Settings.Ins.ServerId}+{Settings.Ins.serverName}】 存数据库失败，先存到磁盘，起服时请务必修改server_config.json配置恢复数据");
                }
                else
                {
                    foreach (var one in list)
                    {
                        var state = one.Replacement;
                        state.SavedToDB();
                    }
                }
            }
        }

        static async Task timerSaveAllStateOfAType()
        {
            //批量回存当前类型的所有state
            var taskList = new List<Task>();
            var changedStateIdEuque = new ConcurrentQueue<long>();
            var batchQueue = new ConcurrentQueue<ReplaceOneModel<BsonDocument>>();
            foreach (var kv in aTypeAllStateMap)
            {
                var state = kv.Value;
                var entity = EntityMgr.GetEntity(state.Id);
                if (entity == null || entity.ReadOnly)
                    continue;

                var comp = await EntityMgr.GetCompAgentByCompType(state.Id, stateCompType);
                var filter = Builders<BsonDocument>.Filter.Eq(MongoField.Id, state.Id);
                var task = comp.Owner.Actor.SendAsync(() => {
                    if (!state.IsChangedComparedToDB())
                        return;

                    var bson = state.ToBsonDocument();
                    state.ReadyToSaveToDB();
                    var saveModel = new ReplaceOneModel<BsonDocument>(filter, bson) { IsUpsert = true };
                    batchQueue.Enqueue(saveModel);
                    changedStateIdEuque.Enqueue(state.Id);

                });
                taskList.Add(task);
            }
            await Task.WhenAll(taskList);

            var batchList = new List<ReplaceOneModel<BsonDocument>>();
            batchList.AddRange(batchQueue);
            var changedStateIdList = new List<long>();
            changedStateIdList.AddRange(changedStateIdEuque);
            var db = MongoDBConnection.Singleton.CurDateBase;
            var col = db.GetCollection<BsonDocument>(BaseDBState.WrapperFullName<TState>());
            int idx = 0;
            int once = 500;
            while (idx < batchList.Count)
            {
                var list = batchList.GetRange(idx, Math.Min(once, batchList.Count - idx));
                var stateIdList = changedStateIdList.GetRange(idx, list.Count);
                idx += once;
                var result = await col.BulkWriteAsync(list, new BulkWriteOptions() { IsOrdered = false });
                if (result.IsAcknowledged)
                {
                    foreach (var id in stateIdList)
                    {
                        aTypeAllStateMap.TryGetValue(id, out var state);
                        if (state == null)
                            continue;
                        var comp = await EntityMgr.GetCompAgentByCompType(state.Id, stateCompType);
                        _ = comp.Owner.Actor.SendAsync(() => {
                            state.SavedToDB();
                        });
                    }
                }
                await Task.Delay(100);
            }
        }

        public async Task ReadStateAsync()
        {
            _State = await MongoDBConnection.Singleton.LoadState<TState>(EntityId);
            IsActive = true;

#if DEBUG_MODE
            _State.SetCompActor(Actor);
#endif
        }

        public Task WriteStateAsync()
        {
            return MongoDBConnection.Singleton.SaveState(_State);
        }

        /// <summary>
        /// 这里不回存，仍然依赖定时回存去回存
        /// </summary>
        public override Task Deactive()
        {
            aTypeAllStateMap.TryRemove(_State.Id, out _);
            return base.Deactive();
        }

        //是不是可以回收了
        internal override Task<bool> ReadyToDeactive()
        {
            if (_State == null)
                return Task.FromResult(true);
            return Task.FromResult(!_State.IsChangedComparedToDB());
        }
    }
}
