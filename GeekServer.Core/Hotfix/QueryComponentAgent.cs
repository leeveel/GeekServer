using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace Geek.Server
{
    public abstract class QueryComponentAgent<TComp> : IComponentAgent where TComp : QueryComponent
    {
        public BaseComponent Owner { get; set; }
        public WorkerActor Actor => Owner.Actor;
        protected TComp Comp => (TComp)Owner;
        public long EntityId => Owner.EntityId;

        public async Task Foreach<T>(IEnumerable<T> itor, Func<T, Task> dealFunc)
        {
            await Actor.SendAsync(async () => {
                foreach (var item in itor)
                {
                    await dealFunc(item);
                }
            });
        }

        public Task<List<T>> Copy<T>(IEnumerable<T> itor)
        {
            return Actor.SendAsync(() => {
                var list = new List<T>(itor);
                return list;
            });
        }

        public virtual Task Active()
        {
            return Task.CompletedTask;
        }

        public virtual Task Deactive()
        {
            return Task.CompletedTask;
        }

        public IMongoDatabase GetDB()
        {
            return Comp.GetDB();
        }

        public Task<T> LoadState<T>(long aId) where T : DBState, new()
        {
            return Comp.LoadState<T>(aId);
        }

        public Task SaveState<T>(T state) where T : DBState
        {
            return Comp.SaveState<T>(state);
        }

        /// <summary>
        /// 查询hash表
        /// </summary>
        public Task<long> QueryHash<T>(string key, string value, FilterDefinition<T> extraFilter) where T : DBState
        {
            return Comp.QueryHash<T>(key, value, extraFilter);
        }

        /// <summary>
        /// 模糊查询
        /// hashKeySearchPattern->abc
        /// </summary>
        public Task<List<T>> QueryHashKeys<T>(string key, string hashKeySearchPattern, int searchNum, FilterDefinition<T> extraFilter) where T : DBState
        {
            return Comp.QueryHashKeys<T>(key, hashKeySearchPattern, searchNum, extraFilter);
        }

        public Task UpdateField<T>(string key, string oldValue, string newValue) where T : DBState
        {
            return Comp.UpdateField<T>(key, oldValue, newValue);
        }
    }
}
