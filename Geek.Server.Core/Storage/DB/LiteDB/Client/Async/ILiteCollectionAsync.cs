using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LiteDB.Async
{
    public interface ILiteCollectionAsync<T>
    {
        /// <summary>
        /// Get collection name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get collection auto id type
        /// </summary>
        BsonAutoId AutoId { get; }

        /// <summary>
        /// Getting entity mapper from current collection. Returns null if collection are BsonDocument type
        /// </summary>
        EntityMapper EntityMapper { get; }

        /// <summary>
        /// Run an include action in each document returned by Find(), FindById(), FindOne() and All() methods to load DbRef documents
        /// Returns a new Collection with this action included
        /// </summary>
        ILiteCollectionAsync<T> Include<K>(Expression<Func<T, K>> keySelector);

        /// <summary>
        /// Run an include action in each document returned by Find(), FindById(), FindOne() and All() methods to load DbRef documents
        /// Returns a new Collection with this action included
        /// </summary>
        ILiteCollectionAsync<T> Include(BsonExpression keySelector);

        /// <summary>
        /// Insert or Update a document in this collection.
        /// </summary>
        Task<bool> UpsertAsync(T entity);

        /// <summary>
        /// Insert or Update all documents
        /// </summary>
        Task<int> UpsertAsync(IEnumerable<T> entities);

        /// <summary>
        /// Insert or Update a document in this collection.
        /// </summary>
        Task<bool> UpsertAsync(BsonValue id, T entity);

        /// <summary>
        /// Update a document in this collection. Returns false if not found document in collection
        /// </summary>
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// Update a document in this collection. Returns false if not found document in collection
        /// </summary>
        Task<bool> UpdateAsync(BsonValue id, T entity);

        /// <summary>
        /// Update all documents
        /// </summary>
        Task<int> UpdateAsync(IEnumerable<T> entities);

        /// <summary>
        /// Update many documents based on transform expression. This expression must return a new document that will be replaced over current document (according with predicate).
        /// Eg: col.UpdateMany("{ Name: UPPER($.Name), Age }", "_id > 0")
        /// </summary>
        Task<int> UpdateManyAsync(BsonExpression transform, BsonExpression predicate);

        /// <summary>
        /// Update many document based on merge current document with extend expression. Use your class with initializers. 
        /// Eg: col.UpdateMany(x => new Customer { Name = x.Name.ToUpper(), Salary: 100 }, x => x.Name == "John")
        /// </summary>
        Task<int> UpdateManyAsync(Expression<Func<T, T>> extend, Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Insert a new entity to this collection. Document Id must be a new value in collection - Returns document Id
        /// </summary>
        Task<BsonValue> InsertAsync(T entity);

        /// <summary>
        /// Insert a new document to this collection using passed id value.
        /// </summary>
        Task InsertAsync(BsonValue id, T entity);

        /// <summary>
        /// Insert an array of new documents to this collection. Document Id must be a new value in collection. Can be set buffer size to commit at each N documents
        /// </summary>
        Task<int> InsertAsync(IEnumerable<T> entities);

        /// <summary>
        /// Implements bulk insert documents in a collection. Usefull when need lots of documents.
        /// </summary>
        Task<int> InsertBulkAsync(IEnumerable<T> entities, int batchSize = 5000);

        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already. Returns true if index was created or false if already exits
        /// </summary>
        /// <param name="name">Index name - unique name for this collection</param>
        /// <param name="expression">Create a custom expression function to be indexed</param>
        /// <param name="unique">If is a unique index</param>
        Task<bool> EnsureIndexAsync(string name, BsonExpression expression, bool unique = false);

        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already. Returns true if index was created or false if already exits
        /// </summary>
        /// <param name="expression">Document field/expression</param>
        /// <param name="unique">If is a unique index</param>
        Task<bool> EnsureIndexAsync(BsonExpression expression, bool unique = false);

        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already.
        /// </summary>
        /// <param name="keySelector">LinqExpression to be converted into BsonExpression to be indexed</param>
        /// <param name="unique">Create a unique keys index?</param>
        Task<bool> EnsureIndexAsync<K>(Expression<Func<T, K>> keySelector, bool unique = false);

        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already.
        /// </summary>
        /// <param name="name">Index name - unique name for this collection</param>
        /// <param name="keySelector">LinqExpression to be converted into BsonExpression to be indexed</param>
        /// <param name="unique">Create a unique keys index?</param>
        Task<bool> EnsureIndexAsync<K>(string name, Expression<Func<T, K>> keySelector, bool unique = false);

        /// <summary>
        /// Drop index and release slot for another index
        /// </summary>
        Task<bool> DropIndexAsync(string name);

        /// <summary>
        /// Return a new LiteQueryable to build more complex queries
        /// </summary>
        ILiteQueryableAsync<T> Query();

        /// <summary>
        /// Find documents inside a collection using predicate expression.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(BsonExpression predicate, int skip = 0, int limit = int.MaxValue);

        /// <summary>
        /// Find documents inside a collection using query definition.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Query query, int skip = 0, int limit = int.MaxValue);

        /// <summary>
        /// Find documents inside a collection using predicate expression.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue);

        /// <summary>
        /// Find a document using Document Id. Returns null if not found.
        /// </summary>
        Task<T> FindByIdAsync(BsonValue id);

        /// <summary>
        /// Find the first document using predicate expression. Returns null if not found
        /// </summary>
        Task<T> FindOneAsync(BsonExpression predicate);

        /// <summary>
        /// Find the first document using predicate expression. Returns null if not found
        /// </summary>
        Task<T> FindOneAsync(string predicate, BsonDocument parameters);

        /// <summary>
        /// Find the first document using predicate expression. Returns null if not found
        /// </summary>
        Task<T> FindOneAsync(BsonExpression predicate, params BsonValue[] args);

        /// <summary>
        /// Find the first document using predicate expression. Returns null if not found
        /// </summary>
        Task<T> FindOneAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Find the first document using defined query structure. Returns null if not found
        /// </summary>
        Task<T> FindOneAsync(Query query);

        /// <summary>
        /// Returns all documents inside collection order by _id index.
        /// </summary>
        Task<IEnumerable<T>> FindAllAsync();

        /// <summary>
        /// Delete a single document on collection based on _id index. Returns true if document was deleted
        /// </summary>
        Task<bool> DeleteAsync(BsonValue id);

        /// <summary>
        /// Delete all documents inside collection. Returns how many documents was deleted. Run inside current transaction
        /// </summary>
        Task<int> DeleteAllAsync();

        /// <summary>
        /// Delete all documents based on predicate expression. Returns how many documents was deleted
        /// </summary>
        Task<int> DeleteManyAsync(BsonExpression predicate);

        /// <summary>
        /// Delete all documents based on predicate expression. Returns how many documents was deleted
        /// </summary>
        Task<int> DeleteManyAsync(string predicate, BsonDocument parameters);

        /// <summary>
        /// Delete all documents based on predicate expression. Returns how many documents was deleted
        /// </summary>
        Task<int> DeleteManyAsync(string predicate, params BsonValue[] args);

        /// <summary>
        /// Delete all documents based on predicate expression. Returns how many documents was deleted
        /// </summary>
        Task<int> DeleteManyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get document count using property on collection.
        /// </summary>
        Task<int> CountAsync();

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any document. Needs indexes on query expression
        /// </summary>
        Task<int> CountAsync(BsonExpression predicate);

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any document. Needs indexes on query expression
        /// </summary>
        Task<int> CountAsync(string predicate, BsonDocument parameters);

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any document. Needs indexes on query expression
        /// </summary>
        Task<int> CountAsync(string predicate, params BsonValue[] args);

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any documents. Needs indexes on query expression
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any documents. Needs indexes on query expression
        /// </summary>
        Task<int> CountAsync(Query query);

        /// <summary>
        /// Get document count using property on collection.
        /// </summary>
        Task<long> LongCountAsync();

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any documents. Needs indexes on query expression
        /// </summary>
        Task<long> LongCountAsync(BsonExpression predicate);

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any documents. Needs indexes on query expression
        /// </summary>
        Task<long> LongCountAsync(string predicate, BsonDocument parameters);

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any documents. Needs indexes on query expression
        /// </summary>
        Task<long> LongCountAsync(string predicate, params BsonValue[] args);

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any documents. Needs indexes on query expression
        /// </summary>
        Task<long> LongCountAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Count documents matching a query. This method does not deserialize any documents. Needs indexes on query expression
        /// </summary>
        Task<long> LongCountAsync(Query query);

        /// <summary>
        /// Returns true if query returns any document. This method does not deserialize any document. Needs indexes on query expression
        /// </summary>
        Task<bool> ExistsAsync(BsonExpression predicate);

        /// <summary>
        /// Returns true if query returns any document. This method does not deserialize any document. Needs indexes on query expression
        /// </summary>
        Task<bool> ExistsAsync(string predicate, BsonDocument parameters);

        /// <summary>
        /// Returns true if query returns any document. This method does not deserialize any document. Needs indexes on query expression
        /// </summary>
        Task<bool> ExistsAsync(string predicate, params BsonValue[] args);

        /// <summary>
        /// Returns true if query returns any document. This method does not deserialize any document. Needs indexes on query expression
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Returns true if query returns any document. This method does not deserialize any document. Needs indexes on query expression
        /// </summary>
        Task<bool> ExistsAsync(Query query);

        /// <summary>
        /// Returns the min value from specified key value in collection
        /// </summary>
        Task<BsonValue> MinAsync(BsonExpression keySelector);

        /// <summary>
        /// Returns the min value of _id index
        /// </summary>
        Task<BsonValue> MinAsync();

        /// <summary>
        /// Returns the min value from specified key value in collection
        /// </summary>
        Task<K> MinAsync<K>(Expression<Func<T, K>> keySelector);

        /// <summary>
        /// Returns the max value from specified key value in collection
        /// </summary>
        Task<BsonValue> MaxAsync(BsonExpression keySelector);

        /// <summary>
        /// Returns the max _id index key value
        /// </summary>
        Task<BsonValue> MaxAsync();

        /// <summary>
        /// Returns the last/max field using a linq expression
        /// </summary>
        Task<K> MaxAsync<K>(Expression<Func<T, K>> keySelector);
    }
}