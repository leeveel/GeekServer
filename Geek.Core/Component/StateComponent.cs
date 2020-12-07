/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using Base;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;
using MongoDB.Driver;
using Geek.Core.Storage;
using MongoDB.Bson;
using Geek.Core.Actor;

namespace Geek.Core.Component
{
    public sealed class StateComponent
    {
        static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        static object lockObj = new object();
        static Func<Task> shutdownFunc;
        static ConcurrentQueue<Func<Task>> timerFuncList = new ConcurrentQueue<Func<Task>>();
        public static void AddShutdownSaveFunc(Func<Task> shutdown, Func<Task> timer)
        {
            lock(lockObj)
            {
                shutdownFunc -= shutdown;
                shutdownFunc += shutdown;

                if (!timerFuncList.Contains(timer))
                    timerFuncList.Enqueue(timer);
            }
        }

        public static async Task SaveAllState()
        {
            try
            {
                var start = DateTime.Now;
                if (shutdownFunc != null)
                    await shutdownFunc();
                LOGGER.Info("all state save time:" + (DateTime.Now - start).TotalMilliseconds);
            }catch(Exception e)
            {
                LOGGER.Info("save all state error");
                LOGGER.Error(e.ToString());
            }
        }

        public static async Task TimerSave()
        {
            try
            {
                var start = DateTime.Now;
                foreach (var func in timerFuncList)
                {
                    await func();
                    if (!DBActor.Singleton.TimerSaving)
                        return;
                    await Task.Delay(500);
                }
                LOGGER.Info("timer save state time:" + (DateTime.Now - start).TotalMilliseconds);
            }catch(Exception e)
            {
                LOGGER.Info("timer save state error");
                LOGGER.Error(e.ToString());
            }
        }
    }

    public abstract class StateComponent<TState> : BaseComponent where TState : CacheState, new()
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        readonly static Random random = new Random();
        public T GetActor<T>() where T : ComponentActor { return (T)Actor; }
        public TState State { get; private set; }
        long storageTimer = 0;
        public override async Task Active()
        {
            if (IsActive)
                return;

            //定时回存
            var savePeriod = random.Next(Settings.Ins.dataFlushTimeMin, Settings.Ins.dataFlushTimeMax) * 1000;
            if (storageTimer > 0)
                Actor.Timer.RemoveTimer(storageTimer);
            if (Settings.Ins.isDebug)
                storageTimer = Actor.Timer.AddTimer(5000, 5000, TimerCheckStateChange);
            else
                storageTimer = Actor.Timer.AddTimer(savePeriod, savePeriod, TimerCheckStateChange);
            await ReadStateAsync();

            StateComponent.AddShutdownSaveFunc(SaveAllState, TimerSaveAllState);
            allStateMap.TryRemove(State._id, out _);
            allStateMap.TryAdd(State._id, State);
        }

        static ConcurrentDictionary<long, TState> allStateMap = new ConcurrentDictionary<long, TState>();
        static async Task SaveAllState()
        {
            Dictionary<long, string> newMd5Map = new Dictionary<long, string>();
            var changedStateList = new List<CacheState>();
            var batchList = new List<ReplaceOneModel<TState>>();
            foreach (var kv in allStateMap)
            {
                var state = kv.Value;
                var actor = await ActorManager.Get<ComponentActor>(state._id);
                if (actor == null || actor.ReadOnly)
                    continue;

                var md5 = state.GetMD5();
                if (state.cacheMD5 == md5)
                    continue;
                newMd5Map[state._id] = md5;
                state.cacheMD5 = md5;
                var filter = Builders<TState>.Filter.Eq(MongoField.UniqueId, state._id);
                var saveModel = new ReplaceOneModel<TState>(filter, state) { IsUpsert = true };
                batchList.Add(saveModel);
                changedStateList.Add(state);
            }

            var db = MongoDBConnection.Singleton.CurDateBase;
            var col = db.GetCollection<TState>(typeof(TState).FullName);
            int idx = 0;
            int once = 500;
            while(idx < batchList.Count)
            {
                var list = batchList.GetRange(idx, Math.Min(once, batchList.Count - idx));
                var stateList = changedStateList.GetRange(idx, list.Count);
                idx += once;
                bool saved = false;
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

                await BackupMgr.Singleton.Backup(stateList, false, !saved);

                if (saved)
                {
                    foreach (var one in list)
                    {
                        var state = one.Replacement;
                        state.cacheMD5 = newMd5Map[state._id];
                    }
                }
                else
                {
                    LOGGER.Fatal("存数据库失败,可能需要从本地回档：" + typeof(TState));
                    BackupMgr.Singleton.SetNeedRestoreNextStartup();
                }
            }
        }

        static async Task TimerSaveAllState()
        {
            var taskList = new List<Task>();
            var changedStateEuque = new ConcurrentQueue<CacheState>();
            var batchQueue = new ConcurrentQueue<ReplaceOneModel<TState>>();
            foreach (var kv in allStateMap)
            {
                var state = kv.Value;
                var actor = await ActorManager.Get<ComponentActor>(state._id);
                if (actor == null || actor.ReadOnly)
                    continue;

                if (string.IsNullOrEmpty(state.toSaveMD5))
                    continue;
                if (state.cacheMD5 == state.toSaveMD5)
                    continue;

                var filter = Builders<TState>.Filter.Eq(MongoField.UniqueId, state._id);
                var task = actor.SendAsync(() => {
                    var str = JsonConvert.SerializeObject(state);
                    var copy = JsonConvert.DeserializeObject<TState>(str);
                    copy.toSaveMD5 = state.toSaveMD5;
                    var saveModel = new ReplaceOneModel<TState>(filter, copy) { IsUpsert = true };
                    batchQueue.Enqueue(saveModel);
                    changedStateEuque.Enqueue(copy);

                });
                taskList.Add(task);
            }
            await Task.WhenAll(taskList);

            var batchList = new List<ReplaceOneModel<TState>>();
            batchList.AddRange(batchQueue);
            var changedStateList = new List<CacheState>();
            changedStateList.AddRange(changedStateEuque);
            var db = MongoDBConnection.Singleton.CurDateBase;
            var col = db.GetCollection<TState>(typeof(TState).FullName);
            int idx = 0;
            int once = 500;
            while (idx < batchList.Count)
            {
                var list = batchList.GetRange(idx, Math.Min(once, batchList.Count - idx));
                var stateList = changedStateList.GetRange(idx, list.Count);
                idx += once;
                var result = await col.BulkWriteAsync(list, new BulkWriteOptions() { IsOrdered = false });
                if (result.IsAcknowledged)
                {
                    foreach (var one in list)
                    {
                        var copy = one.Replacement;
                        allStateMap.TryGetValue(copy._id, out var state);
                        if (state == null)
                            continue;
                        var actor = await ActorManager.Get<ComponentActor>(state._id);
                        _ = actor.SendAsync(() => {
                            state.cacheMD5 = copy.toSaveMD5; //使用copy时的md5
                        });
                    }
                }

                _ = BackupMgr.Singleton.Backup(stateList, true, false);
            }
        }

        long stateLoadedTime;
        /// <summary>其他服的玩家每次获取State数据时调用</summary>
        public async Task ReloadState(int codeTimeInMinutes = 30)
        {
            //更新其他服玩家数据
            //if (!Actor.ReadOnly)
            //    return;
            if ((DateTime.Now - new DateTime(stateLoadedTime)).TotalMinutes < codeTimeInMinutes)
                return;

            stateLoadedTime = DateTime.Now.Ticks;
            State = await MongoDBConnection.Singleton.LoadState<TState>(ActorId);
        }

        async Task ReadStateAsync()
        {
            stateLoadedTime = DateTime.Now.Ticks;
            State = await MongoDBConnection.Singleton.LoadState<TState>(ActorId);
            IsActive = true;
        }

        Task TimerCheckStateChange(Param p)
        {
            if (State != null)
                State.toSaveMD5 = State.GetMD5();
            return Task.CompletedTask;
        }

        public Task WriteStateAsync()
        {
            return MongoDBConnection.Singleton.SaveState(State, true);
        }

        /// <summary>
        /// 这里不回存，仍然依赖定时回存去回存
        /// </summary>
        public override Task Deactive()
        {
            Actor.Timer.RemoveTimer(storageTimer);
            allStateMap.TryRemove(State._id, out _);
            return base.Deactive();
        }

        //是不是可以回收了
        internal override Task<bool> ReadyToDeactive()
        {
            if (State == null)
                return Task.FromResult(true);
            //return Task.FromResult(State.toSaveMD5 == State.cacheMD5);
            return Task.FromResult(State.GetMD5() == State.cacheMD5);
        }
    }
}
