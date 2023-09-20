using System;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace LiteDB.Async
{
    public partial class LiteCollectionAsync<T>
    {
        /// <summary>
        /// Get document count in collection
        /// </summary>
        public Task<int> CountAsync()
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Count()
                );
        }

        /// <summary>
        /// Get document count in collection using predicate filter expression
        /// </summary>
        public Task<int> CountAsync(BsonExpression predicate)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Count(predicate));
        }

        /// <summary>
        /// Get document count in collection using predicate filter expression
        /// </summary>
        public Task<int> CountAsync(string predicate, BsonDocument parameters)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Count(predicate, parameters));
        }

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any documents. Needs indexes on query expression
        /// </summary>
        public Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Count(predicate));
        }

        /// <summary>
        /// Get document count in collection using predicate filter expression
        /// </summary>
        public Task<int> CountAsync(Query query)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Count(query));
        }

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any document. Needs indexes on query expression
        /// </summary>
        public Task<int> CountAsync(string predicate, params BsonValue[] args)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Count(predicate, args));
        }

        /// <summary>
        /// Get document count in collection
        /// </summary>
        public Task<long> LongCountAsync()
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.LongCount());
        }

        /// <summary>
        /// Get document count in collection using predicate filter expression
        /// </summary>
        public Task<long> LongCountAsync(BsonExpression predicate)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.LongCount(predicate));
        }

        /// <summary>
        /// Get document count in collection using predicate filter expression
        /// </summary>
        public Task<long> LongCountAsync(string predicate, BsonDocument parameters)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.LongCount(predicate, parameters));
        }

        /// <summary>
        /// Get document count in collection using predicate filter expression
        /// </summary>
        public Task<long> LongCountAsync(string predicate, params BsonValue[] args)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.LongCount(predicate, args));
        }

        /// <summary>
        /// Get document count in collection using predicate filter expression
        /// </summary>
        public Task<long> LongCountAsync(Expression<Func<T, bool>> predicate)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.LongCount(predicate));
        }

        /// <summary>
        /// Get document count in collection using predicate filter expression
        /// </summary>
        public Task<long> LongCountAsync(Query query)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.LongCount(query));
        }

        /// <summary>
        /// Get true if collection contains at least 1 document that satisfies the predicate expression
        /// </summary>
        public Task<bool> ExistsAsync(BsonExpression predicate)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Exists(predicate));
        }

        /// <summary>
        /// Get true if collection contains at least 1 document that satisfies the predicate expression
        /// </summary>
        public Task<bool> ExistsAsync(string predicate, BsonDocument parameters)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Exists(predicate, parameters));
        }

        /// <summary>
        /// Get true if collection contains at least 1 document that satisfies the predicate expression
        /// </summary>
        public Task<bool> ExistsAsync(string predicate, params BsonValue[] args)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Exists(predicate, args));
        }

        /// <summary>
        /// Get true if collection contains at least 1 document that satisfies the predicate expression
        /// </summary>
        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Exists(predicate));
        }

        /// <summary>
        /// Get true if collection contains at least 1 document that satisfies the predicate expression
        /// </summary>
        public Task<bool> ExistsAsync(Query query)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Exists(query));
        }

        #region Min/Max

        /// <summary>
        /// Returns the min value from specified key value in collection
        /// </summary>
        public Task<BsonValue> MinAsync(BsonExpression keySelector)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Min(keySelector));
        }

        /// <summary>
        /// Returns the min value of _id index
        /// </summary>
        public Task<BsonValue> MinAsync()
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Min());
        }

        /// <summary>
        /// Returns the min value from specified key value in collection
        /// </summary>
        public Task<K> MinAsync<K>(Expression<Func<T, K>> keySelector)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Min(keySelector));
        }

        /// <summary>
        /// Returns the max value from specified key value in collection
        /// </summary>
        public Task<BsonValue> MaxAsync(BsonExpression keySelector)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Max(keySelector));
        }

        /// <summary>
        /// Returns the max _id index key value
        /// </summary>
        public Task<BsonValue> MaxAsync()
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Max());
        }

        /// <summary>
        /// Returns the last/max field using a linq expression
        /// </summary>
        public Task<K> MaxAsync<K>(Expression<Func<T, K>> keySelector)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Max(keySelector));
        }

        #endregion
    }
}
