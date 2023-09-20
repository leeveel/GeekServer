using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LiteDB.async;

namespace LiteDB.Async
{
    /// <summary>
    /// The LiteDB repository pattern. A simple way to access your documents in a single class with fluent query api
    /// </summary>
    public class LiteRepositoryAsync : ILiteRepositoryAsync
    {
        #region Properties

        private readonly ILiteDatabaseAsync _db;

        /// <summary>
        /// Get database instance
        /// </summary>
        public ILiteDatabaseAsync Database => _db;

        #endregion

        #region Ctor

        /// <summary>
        /// Starts LiteDB database an existing Database instance
        /// </summary>
        public LiteRepositoryAsync(ILiteDatabaseAsync database)
        {
            _db = database;
        }

        /// <summary>
        /// Starts LiteDB database using a connection string for file system database
        /// </summary>
        public LiteRepositoryAsync(string connectionString, BsonMapper mapper = null)
        {
            _db = new LiteDatabaseAsync(connectionString, mapper);
        }

        /// <summary>
        /// Starts LiteDB database using a connection string for file system database
        /// </summary>
        public LiteRepositoryAsync(ConnectionString connectionString, BsonMapper mapper = null)
        {
            _db = new LiteDatabaseAsync(connectionString, mapper);
        }

        /// <summary>
        /// Starts LiteDB database using a Stream disk
        /// </summary>
        public LiteRepositoryAsync(Stream stream, BsonMapper mapper = null, Stream logStream = null)
        {
            _db = new LiteDatabaseAsync(stream, mapper, logStream);
        }

        #endregion

        #region Insert

        /// <summary>
        /// Insert a new document into collection. Document Id must be a new value in collection - Returns document Id
        /// </summary>
        public async Task InsertAsync<T>(T entity, string collectionName = null)
        {
            await _db.GetCollection<T>(collectionName).InsertAsync(entity);
        }

        /// <summary>
        /// Insert an array of new documents into collection. Document Id must be a new value in collection. Can be set buffer size to commit at each N documents
        /// </summary>
        public async Task<int> InsertAsync<T>(IEnumerable<T> entities, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).InsertAsync(entities);
        }

        #endregion

        #region Update

        /// <summary>
        /// Update a document into collection. Returns false if not found document in collection
        /// </summary>
        public async Task<bool> UpdateAsync<T>(T entity, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).UpdateAsync(entity);
        }

        /// <summary>
        /// Update all documents
        /// </summary>
        public async Task<int> UpdateAsync<T>(IEnumerable<T> entities, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).UpdateAsync(entities);
        }

        #endregion

        #region Upsert

        /// <summary>
        /// Insert or Update a document based on _id key. Returns true if insert entity or false if update entity
        /// </summary>
        public async Task<bool> UpsertAsync<T>(T entity, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).UpsertAsync(entity);
        }

        /// <summary>
        /// Insert or Update all documents based on _id key. Returns entity count that was inserted
        /// </summary>
        public async Task<int> UpsertAsync<T>(IEnumerable<T> entities, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).UpsertAsync(entities);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Delete entity based on _id key
        /// </summary>
        public async Task<bool> DeleteAsync<T>(BsonValue id, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).DeleteAsync(id);
        }

        /// <summary>
        /// Delete entity based on Query
        /// </summary>
        public async Task<int> DeleteManyAsync<T>(BsonExpression predicate, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).DeleteManyAsync(predicate);
        }

        /// <summary>
        /// Delete entity based on predicate filter expression
        /// </summary>
        public async Task<int> DeleteManyAsync<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).DeleteManyAsync(predicate);
        }

        #endregion

        #region Query

        /// <summary>
        /// Returns new instance of LiteQueryable that provides all method to query any entity inside collection. Use fluent API to apply filter/includes an than run any execute command, like ToList() or First()
        /// </summary>
        public ILiteQueryableAsync<T> Query<T>(string collectionName = null)
        {
            return _db.GetCollection<T>(collectionName).Query();
        }

        #endregion

        #region EnsureIndex

        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already. Returns true if index was created or false if already exits
        /// </summary>
        /// <param name="name">Index name - unique name for this collection</param>
        /// <param name="expression">Create a custom expression function to be indexed</param>
        /// <param name="unique">If is a unique index</param>
        /// <param name="collectionName">Collection Name</param>
        public async Task<bool> EnsureIndexAsync<T>(string name, BsonExpression expression, bool unique = false, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).EnsureIndexAsync(name, expression, unique);
        }

        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already. Returns true if index was created or false if already exits
        /// </summary>
        /// <param name="expression">Create a custom expression function to be indexed</param>
        /// <param name="unique">If is a unique index</param>
        /// <param name="collectionName">Collection Name</param>
        public async Task<bool> EnsureIndexAsync<T>(BsonExpression expression, bool unique = false, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).EnsureIndexAsync(expression, unique);
        }

        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already.
        /// </summary>
        /// <param name="keySelector">LinqExpression to be converted into BsonExpression to be indexed</param>
        /// <param name="unique">Create a unique keys index?</param>
        /// <param name="collectionName">Collection Name</param>
        public async Task<bool> EnsureIndexAsync<T, K>(Expression<Func<T, K>> keySelector, bool unique = false, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).EnsureIndexAsync(keySelector, unique);
        }

        /// <summary>
        /// Create a new permanent index in all documents inside this collections if index not exists already.
        /// </summary>
        /// <param name="name">Index name - unique name for this collection</param>
        /// <param name="keySelector">LinqExpression to be converted into BsonExpression to be indexed</param>
        /// <param name="unique">Create a unique keys index?</param>
        /// <param name="collectionName">Collection Name</param>
        public async Task<bool> EnsureIndexAsync<T, K>(string name, Expression<Func<T, K>> keySelector, bool unique = false, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).EnsureIndexAsync(name, keySelector, unique);
        }

        #endregion

        #region Shortcuts

        /// <summary>
        /// Search for a single instance of T by Id. Shortcut from Query.SingleById
        /// </summary>
        public async Task<T> SingleByIdAsync<T>(BsonValue id, string collectionName = null)
        {
            return await _db.GetCollection<T>(collectionName).Query()
                .Where("_id = @0", id)
                .SingleAsync();
        }

        /// <summary>
        /// Execute Query[T].Where(predicate).ToList();
        /// </summary>
        public async Task<List<T>> FetchAsync<T>(BsonExpression predicate, string collectionName = null)
        {
            return await Query<T>(collectionName)
                .Where(predicate)
                .ToListAsync();
        }

        /// <summary>
        /// Execute Query[T].Where(predicate).ToList();
        /// </summary>
        public async Task<List<T>> FetchAsync<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return await Query<T>(collectionName)
                .Where(predicate)
                .ToListAsync();
        }

        /// <summary>
        /// Execute Query[T].Where(predicate).First();
        /// </summary>
        public async Task<T> FirstAsync<T>(BsonExpression predicate, string collectionName = null)
        {
            return await Query<T>(collectionName)
                .Where(predicate)
                .FirstAsync();
        }

        /// <summary>
        /// Execute Query[T].Where(predicate).First();
        /// </summary>
        public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return await Query<T>(collectionName)
                .Where(predicate)
                .FirstAsync();
        }

        /// <summary>
        /// Execute Query[T].Where(predicate).FirstOrDefault();
        /// </summary>
        public async Task<T> FirstOrDefaultAsync<T>(BsonExpression predicate, string collectionName = null)
        {
            return await Query<T>(collectionName)
                .Where(predicate)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Execute Query[T].Where(predicate).FirstOrDefault();
        /// </summary>
        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return await Query<T>(collectionName)
                .Where(predicate)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Execute Query[T].Where(predicate).Single();
        /// </summary>
        public async Task<T> SingleAsync<T>(BsonExpression predicate, string collectionName = null)
        {
            return await Query<T>(collectionName)
                .Where(predicate)
                .SingleAsync();
        }

        /// <summary>
        /// Execute Query[T].Where(predicate).Single();
        /// </summary>
        public async Task<T> SingleAsync<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return await Query<T>(collectionName)
                .Where(predicate)
                .SingleAsync();
        }

        /// <summary>
        /// Execute Query[T].Where(predicate).SingleOrDefault();
        /// </summary>
        public async Task<T> SingleOrDefaultAsync<T>(BsonExpression predicate, string collectionName = null)
        {
            return await Query<T>(collectionName)
                .Where(predicate)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Execute Query[T].Where(predicate).SingleOrDefault();
        /// </summary>
        public async Task<T> SingleOrDefaultAsync<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return await Query<T>(collectionName)
                .Where(predicate)
                .SingleOrDefaultAsync();
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LiteRepositoryAsync()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Ensure UnderlyingDatabase is immediately disposed of..._
                _db.UnderlyingDatabase.Dispose();
                _db.Dispose();
            }
        }
    }
}