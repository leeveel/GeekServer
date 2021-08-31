using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class QueryComponentAgent<TComp> : IComponentAgent where TComp : QueryComponent
    {
        public object Owner { get; set; }
        protected TComp Comp => (TComp)Owner;

        public ComponentActor Actor => Comp.Actor;

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
