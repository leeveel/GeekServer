using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LiteDB.Async
{
    public interface ILiteQueryableAsync<T> : ILiteQueryableAsyncResult<T>
    {
        /// <summary>
        /// Load cross reference documents from path expression (DbRef reference)
        /// </summary>
        ILiteQueryableAsync<T> Include(BsonExpression path);

        /// <summary>
        /// Load cross reference documents from path expression (DbRef reference)
        /// </summary>
        ILiteQueryableAsync<T> Include(List<BsonExpression> paths);

        /// <summary>
        /// Load cross reference documents from path expression (DbRef reference)
        /// </summary>
        ILiteQueryableAsync<T> Include<K>(Expression<Func<T, K>> path);

        /// <summary>
        /// Filters a sequence of documents based on a predicate expression
        /// </summary>
        ILiteQueryableAsync<T> Where(BsonExpression predicate);

        /// <summary>
        /// Filters a sequence of documents based on a predicate expression
        /// </summary>
        ILiteQueryableAsync<T> Where(string predicate, BsonDocument parameters);

        /// <summary>
        /// Filters a sequence of documents based on a predicate expression
        /// </summary>
        ILiteQueryableAsync<T> Where(string predicate, params BsonValue[] args);

        /// <summary>
        /// Filters a sequence of documents based on a predicate expression
        /// </summary>
        ILiteQueryableAsync<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Sort the documents of resultset in ascending (or descending) order according to a key (support only one OrderBy)
        /// </summary>
        ILiteQueryableAsync<T> OrderBy(BsonExpression keySelector, int order = 1);

        /// <summary>
        /// Sort the documents of resultset in ascending (or descending) order according to a key (support only one OrderBy)
        /// </summary>
        ILiteQueryableAsync<T> OrderBy<K>(Expression<Func<T, K>> keySelector, int order = 1);

        /// <summary>
        /// Sort the documents of resultset in descending order according to a key (support only one OrderBy)
        /// </summary>
        ILiteQueryableAsync<T> OrderByDescending(BsonExpression keySelector);

        /// <summary>
        /// Sort the documents of resultset in descending order according to a key (support only one OrderBy)
        /// </summary>
        ILiteQueryableAsync<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector);

        /// <summary>
        /// Groups the documents of resultset according to a specified key selector expression (support only one GroupBy)
        /// </summary>
        ILiteQueryableAsync<T> GroupBy(BsonExpression keySelector);

        /// <summary>
        /// Filter documents after group by pipe according to predicate expression (requires GroupBy and support only one Having)
        /// </summary>
        ILiteQueryableAsync<T> Having(BsonExpression predicate);

        /// <summary>
        /// Transform input document into a new output document. Can be used with each document, group by or all source
        /// </summary>
        ILiteQueryableAsyncResult<BsonDocument> Select(BsonExpression selector);

        /// <summary>
        /// Project each document of resultset into a new document/value based on selector expression
        /// </summary>
        ILiteQueryableAsyncResult<K> Select<K>(Expression<Func<T, K>> selector);
    }
}
