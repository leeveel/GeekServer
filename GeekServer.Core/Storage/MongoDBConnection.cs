using System;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class MongoDBConnection
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static readonly MongoDBConnection Singleton = new MongoDBConnection();
        public IMongoDatabase CurDateBase { get; private set; }
        public MongoClient Client { get; private set; }
        public void Connect(string db, string connectConfig)
        {
            MongoClient client = new MongoClient(connectConfig);
            CurDateBase = client.GetDatabase(db);
            client.ListDatabaseNames();
        }

        public async Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : DBState, new()
        {
            try
            {
                var look = EntityStateSeeker.GetState<TState>(id);
                if (look != null)
                    return look;

                //读数据
                var filter = Builders<TState>.Filter.Eq(MongoField.Id, id);
                var col = CurDateBase.GetCollection<TState>(BaseDBState.WrapperFullName<TState>());
                var state = await (await col.FindAsync(filter)).FirstOrDefaultAsync();
                if (state == null && defaultGetter != null)
                    state = defaultGetter();
                if (state == null)
                {
                    //state = new TState { Id = id };
                    state = (TState)BaseDBState.CreateStateWrapper<TState>();
                    state.Id = id;
                }
                state.ClearChanges();
                return state;
            }
            catch(Exception e)
            {
                LOGGER.Fatal(e.ToString());
                //await Task.Delay(500);
                //return await LoadState<TState>(id, defaultGetter);
                return default;
            }
        }

        public async Task<TState> LoadState<TState>(string id, Func<TState> defaultGetter = null) where TState : BaseDBState
        {
            try
            {
                //读数据
                var filter = Builders<TState>.Filter.Eq(MongoField.Id, id);
                var col = CurDateBase.GetCollection<TState>(BaseDBState.WrapperFullName<TState>());
                var state = await (await col.FindAsync(filter)).FirstOrDefaultAsync();
                if (state == null && defaultGetter != null)
                    state = defaultGetter();
                if (state != null)
                    state.ClearChanges();
                return state;
            }
            catch (Exception e)
            {
                LOGGER.Fatal(e.ToString());
                //await Task.Delay(500);
                //return await LoadState<TState>(id, defaultGetter);
                return default;
            }
        }

        public async Task SaveState<TState>(TState state) where TState : DBState
        {
            if (state.IsChangedComparedToDB())
            {
                state.ReadyToSaveToDB();
                //保存数据
                var filter = Builders<TState>.Filter.Eq(MongoField.Id, state.Id);
                var col = CurDateBase.GetCollection<TState>(BaseDBState.WrapperFullName<TState>());
                var ret = await col.ReplaceOneAsync(filter, state, new ReplaceOptions() { IsUpsert = true });
                if (ret.IsAcknowledged)
                    state.SavedToDB();
            }
        }

        public async Task SaveState<TState>(string id, TState state) where TState : BaseDBState
        {
            if (state.IsChangedComparedToDB())
            {
                state.ReadyToSaveToDB();
                //保存数据
                var filter = Builders<TState>.Filter.Eq(MongoField.Id, id);
                var col = CurDateBase.GetCollection<TState>(BaseDBState.WrapperFullName<TState>());
                var ret = await col.ReplaceOneAsync(filter, state, new ReplaceOptions() { IsUpsert = true });
                if (ret.IsAcknowledged)
                    state.SavedToDB();
            }
        }

        public Task IndexCollectoinMore<TState>(string indexKey) where TState : DBState
        {
            var col = CurDateBase.GetCollection<TState>(BaseDBState.WrapperFullName<TState>());
            var key1 = Builders<TState>.IndexKeys.Ascending(indexKey);
            var key2 = Builders<TState>.IndexKeys.Ascending(MongoField.Id);
            var model1 = new CreateIndexModel<TState>(key1);
            var model2 = new CreateIndexModel<TState>(key2);
            return col.Indexes.CreateManyAsync(new List<CreateIndexModel<TState>>() { model1, model2 });
        }
    }
}