using System;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace LiteDB.Async
{
    public partial class LiteCollectionAsync<T>
    {
        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already. Returns true if index was created or false if already exits
        /// </summary>
        /// <param name="name">Index name - unique name for this collection</param>
        /// <param name="expression">Create a custom expression function to be indexed</param>
        /// <param name="unique">If is a unique index</param>
        public Task<bool> EnsureIndexAsync(string name, BsonExpression expression, bool unique = false)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.EnsureIndex(name, expression, unique));
        }

        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already. Returns true if index was created or false if already exits
        /// </summary>
        /// <param name="expression">Document field/expression</param>
        /// <param name="unique">If is a unique index</param>
        public Task<bool> EnsureIndexAsync(BsonExpression expression, bool unique = false)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.EnsureIndex(expression, unique));
        }

        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already.
        /// </summary>
        /// <param name="keySelector">LinqExpression to be converted into BsonExpression to be indexed</param>
        /// <param name="unique">Create a unique keys index?</param>
        public Task<bool> EnsureIndexAsync<K>(Expression<Func<T, K>> keySelector, bool unique = false)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.EnsureIndex(keySelector, unique));
        }

        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already.
        /// </summary>
        /// <param name="name">Index name - unique name for this collection</param>
        /// <param name="keySelector">LinqExpression to be converted into BsonExpression to be indexed</param>
        /// <param name="unique">Create a unique keys index?</param>
        public Task<bool> EnsureIndexAsync<K>(string name, Expression<Func<T, K>> keySelector, bool unique = false)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.EnsureIndex<K>(name, keySelector, unique));
        }

        /// <summary>
        /// Drop index and release slot for another index
        /// </summary>
        public Task<bool> DropIndexAsync(string name)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.DropIndex(name));
        }
    }
}