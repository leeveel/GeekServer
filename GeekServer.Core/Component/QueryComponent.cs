using System;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class QueryComponent : BaseComponent
    {
        public IMongoDatabase GetDB()
        {
            return MongoDBConnection.Singleton.CurDateBase;
        }

        public Task<T> LoadState<T>(long id, Func<T> defaultGetter = null) where T : DBState, new()
        {
            return MongoDBConnection.Singleton.LoadState<T>(id, defaultGetter);
        }

        public Task<T> LoadState<T>(string id, Func<T> defaultGetter = null) where T : InnerDBState
        {
            return MongoDBConnection.Singleton.LoadState<T>(id, defaultGetter);
        }

        public Task SaveState<T>(T state) where T : DBState
        {
            return MongoDBConnection.Singleton.SaveState(state);
        }

        public Task SaveState<T>(string id, T state) where T : InnerDBState
        {
            return MongoDBConnection.Singleton.SaveState(id, state);
        }

        /// <summary>
        /// 查询hash表
        /// </summary>
        public async Task<long> QueryHash<T>(string key, string value, FilterDefinition<T> extraFilter) where T : DBState
        {
            var col = GetDB().GetCollection<T>(typeof(T).FullName);
            var filter = Builders<T>.Filter.Eq(key, value);

            if (extraFilter != null)
                filter &= extraFilter;
            var project = Builders<T>.Projection.Include(MongoField.Id).Include(key);

            var option = new FindOptions<T, T>();
            option.Limit = 1;
            option.Projection = project;
            var ret = await col.FindAsync(filter, option);
            var state = await ret.FirstOrDefaultAsync();
            if (state != null)
                return state.Id;
            return 0;
        }

        /// <summary>
        /// 模糊查询
        /// hashKeySearchPattern->abc
        /// </summary>
        public async Task<List<T>> QueryHashKeys<T>(string key, string hashKeySearchPattern, int searchNum, FilterDefinition<T> extraFilter) where T : DBState
        {
            var col = GetDB().GetCollection<T>(typeof(T).FullName);
            var filter = Builders<T>.Filter.Regex(key, hashKeySearchPattern);
            if (extraFilter != null)
                filter &= extraFilter;

            var project = Builders<T>.Projection.Include(MongoField.Id).Include(key);
            var option = new FindOptions<T, T>();
            option.Limit = searchNum;
            option.Projection = project;
            var ret = await col.FindAsync(filter, option);
            return await ret.ToListAsync();
        }

        public async Task UpdateField<T>(string key, string oldValue, string newValue) where T : DBState
        {
            var col = GetDB().GetCollection<T>(typeof(T).FullName);
            var filter = Builders<T>.Filter.Eq(key, oldValue);
            var update = Builders<T>.Update.Set(key, newValue);
            await col.FindOneAndUpdateAsync(filter, update);
        }
    }
}
