using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NLog;

namespace Geek.Server.Core.Storage
{

    public static class MongoDBExtensions
    {
        public static IMongoCollection<TDocument> GetCollection<TDocument>(this IMongoDatabase self, MongoCollectionSettings settings = null)
        {
            return self.GetCollection<TDocument>(typeof(TDocument).FullName, settings);
        }
    }

    public class MongoDBConnection : IGameDB
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();

        public MongoClient Client { get; private set; }

        public IMongoDatabase CurDB { get; private set; }

        public void Open(string url, string dbName)
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

        public async Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, id);
            var col = CurDB.GetCollection<TState>();
            using var cursor = await col.FindAsync(filter);
            var state = await cursor.FirstOrDefaultAsync();
            bool isNew = state == null;
            if (state == null && defaultGetter != null)
                state = defaultGetter();
            if (state == null)
                state = new TState { Id = id };
            state.AfterLoadFromDB(isNew);
            return state;
        }

        public async Task SaveState<TState>(TState state) where TState : CacheState
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

        public Task CreateIndex<TState>(string indexKey)
        {
            var col = CurDB.GetCollection<TState>();
            var key = Builders<TState>.IndexKeys.Ascending(indexKey);
            var model = new CreateIndexModel<TState>(key);
            return col.Indexes.CreateOneAsync(model);
        }

        public void Close()
        {
            Client.Cluster.Dispose();
        }
        
    }
}
