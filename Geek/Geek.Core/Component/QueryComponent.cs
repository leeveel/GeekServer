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
using Geek.Core.Storage;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Core.Component
{
    public abstract class QueryComponent : BaseComponent
    {
        public IMongoDatabase GetDB()
        {
            return MongoDBConnection.Singleton.CurDateBase;
        }

        public Task<T> LoadState<T>(long aId) where T : CacheState, new()
        {
            return MongoDBConnection.Singleton.LoadState<T>(aId);
        }

        public Task SaveState<T>(T state) where T : CacheState
        {
            return MongoDBConnection.Singleton.SaveState(state, true);
        }

        /// <summary>
        /// 查询hash表
        /// </summary>
        public async Task<long> QueryHash<T>(string key, string value, FilterDefinition<T> extraFilter) where T : CacheState
        {
            var col = GetDB().GetCollection<T>(typeof(T).FullName);
            var filter = Builders<T>.Filter.Eq(key, value);

            if (extraFilter != null)
                filter &= extraFilter;
            var project = Builders<T>.Projection.Include(MongoField.UniqueId).Include(key);

            var option = new FindOptions<T, T>();
            option.Limit = 1;
            option.Projection = project;
            var ret = await col.FindAsync(filter, option);
            var state = await ret.FirstOrDefaultAsync();
            if (state != null)
                return state._id;
            return 0;
        }

        /// <summary>
        /// 模糊查询
        /// hashKeySearchPattern->abc
        /// </summary>
        public async Task<List<T>> QueryHashKeys<T>(string key, string hashKeySearchPattern, int searchNum, FilterDefinition<T> extraFilter) where T : CacheState
        {
            var col = GetDB().GetCollection<T>(typeof(T).FullName);
            var filter = Builders<T>.Filter.Regex(key, hashKeySearchPattern);
            if (extraFilter != null)
                filter &= extraFilter;

            var project = Builders<T>.Projection.Include(MongoField.UniqueId).Include(key);
            var option = new FindOptions<T, T>();
            option.Limit = searchNum;
            option.Projection = project;
            var ret = await col.FindAsync(filter, option);
            return await ret.ToListAsync();
        }

        public async Task UpdateField<T>(string key, string oldValue, string newValue) where T : CacheState
        {
            var col = GetDB().GetCollection<T>(typeof(T).FullName);
            var filter = Builders<T>.Filter.Eq(key, oldValue);
            var update = Builders<T>.Update.Set(key, newValue);
            await col.FindOneAndUpdateAsync(filter, update);
        }
    }
}
