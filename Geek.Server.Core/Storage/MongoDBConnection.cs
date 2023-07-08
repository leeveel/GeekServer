using Geek.Server.Core.Serialize;
using Geek.Server.Core.Utils;
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

        public static readonly ReplaceOptions REPLACE_OPTIONS = new() { IsUpsert = true };

        public static readonly BulkWriteOptions BULK_WRITE_OPTIONS = new() { IsOrdered = false };

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
            var filter = Builders<MongoState>.Filter.Eq(CacheState.UniqueId, id);
            var stateName = typeof(TState).FullName;
            var col = CurDB.GetCollection<MongoState>(stateName);

            using var cursor = await col.FindAsync(filter);
            var mongoState = await cursor.FirstOrDefaultAsync();
            bool isNew = mongoState == null;
            TState state = default;
            if (mongoState != null)
            {
                try
                {
                    state = Serializer.Deserialize<TState>(mongoState.Data);
                }
                catch (Exception e)
                {
                    Log.Error($"从mongodb的{stateName} {id}加载数据出错:{e.Message}");
                }
            }
            if (mongoState == null && defaultGetter != null)
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
                var mongoState = new MongoState()
                {
                    Data = data,
                    Id = state.Id.ToString(),
                    Timestamp = TimeUtils.CurrentTimeMillisUTC()
                };
                var filter = Builders<MongoState>.Filter.Eq(CacheState.UniqueId, mongoState.Id);
                var stateName = typeof(TState).FullName;
                var col = CurDB.GetCollection<MongoState>(stateName);
                var result = await col.ReplaceOneAsync(filter, mongoState, REPLACE_OPTIONS);
                if (result.IsAcknowledged)
                {
                    state.AfterSaveToDB();
                }
            }
        }


        public void Close()
        {
            Client.Cluster.Dispose();
        }

        public void Flush(bool wait)
        {

        }
    }
}