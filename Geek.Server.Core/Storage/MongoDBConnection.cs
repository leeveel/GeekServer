using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NLog;

namespace Geek.Server
{
    public static class MongoDBConnection
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static MongoClient Client { get; private set; }

        public static IMongoDatabase CurDB { get; private set; }

        public static void Init(string url, string dbName)
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString(url);
                Client = new MongoClient(settings);
                CurDB = Client.GetDatabase(dbName);
                Log.Info($"初始化MongoDB服务完成 Url:{url} DbName:{dbName}");
            }
            catch (Exception)
            {
                Log.Error($"初始化MongoDB服务失败 Url:{url} DbName:{dbName}");
                throw;
            }
        }

        public static readonly StatisticsTool statisticsTool = new();

        public static async Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            statisticsTool.Count(typeof(TState).FullName);
            var state = RoleStateLoader.GetState<TState>(id);
            bool isNew;
            if (state == null)
            {
                var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, id);
                var col = CurDB.GetCollection<TState>();
                using var cursor = await col.FindAsync(filter);
                state = await cursor.FirstOrDefaultAsync();
                isNew = state == null;
                if (state == null && defaultGetter != null)
                    state = defaultGetter();
                if (state == null)
                    state = new TState { Id = id };
            }
            else
            {
                isNew = false;
            }

            state.AfterLoadFromDB(isNew);
            return state;
        }

        public static async Task SaveState<TState>(TState state) where TState : CacheState
        {
            var (isChanged, data) = state.IsChanged();
            if (isChanged)
            {
                var _state = BsonSerializer.Deserialize<TState>(data);
                var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, state.Id);
                var col = CurDB.GetCollection<TState>();
                var result = await col.ReplaceOneAsync(filter, _state, REPLACE_OPTIONS);
                if (result.IsAcknowledged)
                {
                    state.AfterSaveToDB();
                }
            }
        }

        public static readonly ReplaceOptions REPLACE_OPTIONS = new() { IsUpsert = true };

        public static readonly BulkWriteOptions BULK_WRITE_OPTIONS = new() { IsOrdered = false };

        public static Task CreateIndex<TState>(string indexKey)
        {
            var col = CurDB.GetCollection<TState>();
            var key = Builders<TState>.IndexKeys.Ascending(indexKey);
            var model = new CreateIndexModel<TState>(key);
            return col.Indexes.CreateOneAsync(model);
        }
    }
}
