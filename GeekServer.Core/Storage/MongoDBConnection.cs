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
        public void Connect(string db, string connectConfig)
        {
            MongoClient client = new MongoClient(connectConfig);
            CurDateBase = client.GetDatabase(db);
        }

        public async Task<TState> LoadState<TState>(long aId) where TState : DBState, new()
        {
            try
            {
                //读数据
                var filter = Builders<TState>.Filter.Eq(MongoField.UniqueId, aId);
                var col = CurDateBase.GetCollection<TState>(typeof(TState).FullName);
                var state = await (await col.FindAsync(filter)).FirstOrDefaultAsync();
                if (state == null)
                    state = new TState { _id = aId };
                state.ClearChanges();
                return state;
            }catch(Exception e)
            {
                LOGGER.Fatal(e.ToString());
                await Task.Delay(500);
                return await LoadState<TState>(aId);
            }
        }

        public async Task SaveState<TState>(TState state) where TState : DBState
        {
            if (state.IsChangedRefDB())
            {
                state.UpdateChangeVersion();
                state.ReadyToSaveToDB();
                //保存数据
                var filter = Builders<TState>.Filter.Eq(MongoField.UniqueId, state._id);
                var col = CurDateBase.GetCollection<TState>(typeof(TState).FullName);
                var ret = await col.ReplaceOneAsync(filter, state, new ReplaceOptions() { IsUpsert = true });
                if (ret.IsAcknowledged)
                    state.SavedToDB();
            }
        }

        public Task IndexCollectoinMore<TState>(string indexKey) where TState : DBState
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