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
            client.ListDatabaseNames();
        }

        public async Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : DBState, new()
        {
            try
            {
                //读数据
                var filter = BuildFilter<TState>(id);
                var col = BuildCol<TState>();
                var state = await (await col.FindAsync(filter)).FirstOrDefaultAsync();
                if (state == null && defaultGetter != null)
                    state = defaultGetter();
                if (state == null)
                    state = new TState { Id = id };
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

        public async Task<TState> LoadState<TState>(string id, Func<TState> defaultGetter = null) where TState : InnerDBState
        {
            try
            {
                //读数据
                var filter = BuildFilter<TState>(id);
                var col = BuildCol<TState>();
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

        public Task SaveState<TState>(TState state) where TState : DBState
        {
            return SaveState(state.Id, state);
        }

        public async Task SaveState<TState>(object id, TState state) where TState : InnerDBState
        {
            if (state.IsChanged)
            {
                //保存数据
                var filter = BuildFilter<TState>(id);
                var col = BuildCol<TState>();
                var update = BuildUpdateDefinition(typeof(TState), state);
                await col.UpdateOneAsync(filter, update, new UpdateOptions() { IsUpsert = true });
            }
        }

        private IMongoCollection<TState> BuildCol<TState>() where TState : InnerDBState
        {
            return CurDateBase.GetCollection<TState>(typeof(TState).FullName);
        }

        public static UpdateOneModel<TState> BuildUpdateOneModel<TState>(Type type, TState state, object id) where TState : InnerDBState
        {
            var filter = BuildFilter<TState>(id);
            var update = BuildUpdateDefinition(type, state);
            return new UpdateOneModel<TState>(filter, update) { IsUpsert = true };
        }

        private static FilterDefinition<TState> BuildFilter<TState>(object id) where TState : InnerDBState
        {
            return Builders<TState>.Filter.Eq(MongoField.Id, id);
        }

        private static UpdateDefinition<TState> BuildUpdateDefinition<TState>(Type type, TState state) where TState : InnerDBState
        {
            UpdateDefinition<TState> update = null;
            foreach (var p in type.GetProperties())
            {
                var v = p.GetValue(state);
                if (state.changedSet.Contains(p.Name))
                {
                    UpdateModel(ref update, p.Name, v);
                }
                else if (v is InnerDBState ids)
                {
                    foreach (var pp in p.PropertyType.GetProperties())
                    {
                        var vv = pp.GetValue(v);

                        if (ids.changedSet.Contains(pp.Name)
                        || (vv is BaseState vvds && vvds.IsChanged))
                        {
                            UpdateModel(ref update, $"{p.Name}.{pp.Name}", vv);
                        }
                    }
                }
                else if (v is BaseState ds && ds.IsChanged)
                {
                    UpdateModel(ref update, p.Name, v);
                }
            }

            return update;
        }

        private static void UpdateModel<TState>(ref UpdateDefinition<TState> update, string name, object v) where TState : InnerDBState
        {
            if (update == null)
            {
                update = Builders<TState>.Update.Set(name, v);
            }
            else
            {
                update = update.Set(name, v);
            }
        }

        public Task IndexCollectoinMore<TState>(string indexKey) where TState : DBState
        {
            var col = CurDateBase.GetCollection<TState>(typeof(TState).FullName);
            var key1 = Builders<TState>.IndexKeys.Ascending(indexKey);
            var key2 = Builders<TState>.IndexKeys.Ascending(MongoField.Id);
            var model1 = new CreateIndexModel<TState>(key1);
            var model2 = new CreateIndexModel<TState>(key2);
            return col.Indexes.CreateManyAsync(new List<CreateIndexModel<TState>>() { model1, model2 });
        }
    }
}