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
using System;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Geek.Core.Storage
{
    public class MongoDBConnection
    {
        static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static readonly MongoDBConnection Singleton = new MongoDBConnection();
        public IMongoDatabase CurDateBase { get; private set; }
        public void Connect(string db, string connectConfig)
        {
            MongoClient client = new MongoClient(connectConfig);
            CurDateBase = client.GetDatabase(db);
        }

        Dictionary<Type, Dictionary<long, CacheState>> cacheStateMap = new Dictionary<Type, Dictionary<long, CacheState>>();
        /// <summary>批量加载到缓存,用于起服批量获取活跃玩家数据，游戏内逻辑不要用</summary>
        public async Task BatchLoadStateToCache<TState>(List<long> idList) where TState : CacheState
        {
            if (idList == null || idList.Count <= 0)
                return;

            if (!cacheStateMap.ContainsKey(typeof(TState)))
                cacheStateMap[typeof(TState)] = new Dictionary<long, CacheState>();
            var map = cacheStateMap[typeof(TState)];

            FilterDefinition<TState> filter = null;
            foreach (var id in idList)
            {
                if(filter == null)
                    filter = Builders<TState>.Filter.Eq(MongoField.UniqueId, id);
                else
                    filter |= Builders<TState>.Filter.Eq(MongoField.UniqueId, id);
            }
            var col = CurDateBase.GetCollection<TState>(typeof(TState).FullName);
            var stateList = await col.FindAsync(filter);
            await stateList.ForEachAsync((state) => {
                map[state._id] = state;
                state.cacheMD5 = state.GetMD5();
            });
        }
        
        /// <summary>获取缓存的状态</summary>
        TState GetCacheState<TState>(long id) where TState : CacheState
        {
            if (cacheStateMap == null)
                return default;
            if(cacheStateMap.ContainsKey(typeof(TState)))
            {
                var map = cacheStateMap[typeof(TState)];
                if (map.ContainsKey(id))
                    return (TState)map[id];
            }
            return default;
        }
        /// <summary>清除缓存</summary>
        public void ClearCacheState()
        {
            cacheStateMap = null;
        }


        int OperatingNum;
        public async Task<TState> LoadState<TState>(long aId) where TState : CacheState, new()
        {
            var cacheState = GetCacheState<TState>(aId);
            if (cacheState != null)
                return cacheState;

            while (OperatingNum > Settings.Ins.dataFPS)
                await Task.Delay(100);
            Interlocked.Increment(ref OperatingNum);

            try
            {
                //读数据
                var filter = Builders<TState>.Filter.Eq(MongoField.UniqueId, aId);
                var col = CurDateBase.GetCollection<TState>(typeof(TState).FullName);
                var state = await (await col.FindAsync(filter)).FirstOrDefaultAsync();
                if (state == null)
                    state = new TState { _id = aId };
                else
                    state.cacheMD5 = state.GetMD5();
                Interlocked.Decrement(ref OperatingNum);
                return state;
            }catch(Exception e)
            {
                LOGGER.Fatal(e.ToString());
                await Task.Delay(500);
                Interlocked.Decrement(ref OperatingNum);
                return await LoadState<TState>(aId);
            }
        }

        public async Task SaveState<TState>(TState state, bool forceImmediately) where TState : CacheState
        {
            string newMd5 = state.GetMD5();
            if (newMd5 != state.cacheMD5)
            {
                //限流
                if(!forceImmediately)
                {
                    while (OperatingNum > Settings.Ins.dataFPS)
                        await Task.Delay(100);
                }
                Interlocked.Increment(ref OperatingNum);


                //保存数据
                var filter = Builders<TState>.Filter.Eq(MongoField.UniqueId, state._id);
                var col = CurDateBase.GetCollection<TState>(typeof(TState).FullName);
                var ret = await col.ReplaceOneAsync(filter, state, new ReplaceOptions() { IsUpsert = true });
                if (ret.IsAcknowledged)
                    state.cacheMD5 = newMd5;
                Interlocked.Decrement(ref OperatingNum);
            }
        }

        public Task IndexCollectoinMore<TState>(string indexKey) where TState : CacheState
        {
            var col = CurDateBase.GetCollection<TState>(typeof(TState).FullName);
            var key1 = Builders<TState>.IndexKeys.Ascending(indexKey);
            var key2 = Builders<TState>.IndexKeys.Ascending(MongoField.UniqueId);
            var model1 = new CreateIndexModel<TState>(key1);
            var model2 = new CreateIndexModel<TState>(key2);
            return col.Indexes.CreateManyAsync(new List<CreateIndexModel<TState>>() { model1, model2 });
        }
    }
}