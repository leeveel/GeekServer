using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using LiteDB.Engine;

namespace LiteDB.Async
{
    public interface ILiteDatabaseAsync : IDisposable
    {
        bool UtcDate { get; set; }

        int CheckpointSize { get; set; }

        int UserVersion { get; set; }

        TimeSpan Timeout { get; set; }

        Collation Collation  { get; }

        long LimitSize { get; set; }

        #region Collections
        /// <summary>
        /// Get a collection using a name based on typeof(T).Name (BsonMapper.ResolveCollectionName function)
        /// </summary>
        ILiteCollectionAsync<T> GetCollection<T>();

        /// <summary>
        /// Get a collection using a entity class as strong typed document. If collection does not exits, create a new one.
        /// </summary>
        /// <param name="name">Collection name (case insensitive)</param>
        ILiteCollectionAsync<T> GetCollection<T>(string name);

        /// <summary>
        /// Get a collection using a generic BsonDocument. If collection does not exits, create a new one.
        /// </summary>
        /// <param name="name">Collection name (case insensitive)</param>
        /// <param name="autoId">Define autoId data type (when document contains no _id field)</param>
        ILiteCollectionAsync<BsonDocument> GetCollection(string name, BsonAutoId autoId = BsonAutoId.ObjectId);
        #endregion

        #region FileStorage

        /// <summary>
        /// Returns a special collection for storage files/stream inside datafile. Use _files and _chunks collection names. FileId is implemented as string. Use "GetStorage" for custom options
        /// </summary>
        ILiteStorageAsync<string> FileStorage { get; }

        /// <summary>
        /// Gets the underlying <see cref="ILiteDatabase"/>. Useful to access various operations not exposed by <see cref="ILiteDatabaseAsync"/>
        /// </summary>
        ILiteDatabase UnderlyingDatabase { get; }

        /// <summary>
        /// Get new instance of Storage using custom FileId type, custom "_files" collection name and custom "_chunks" collection. LiteDB support multiples file storages (using different files/chunks collection names)
        /// </summary>
        ILiteStorageAsync<TFileId> GetStorage<TFileId>(string filesCollection = "_files", string chunksCollection = "_chunks");

        #endregion

        #region Transactions
        /// <summary>
        /// Return another database sharing the same file as this one and in a transaction
        /// </summary>
        /// <returns></returns>
        Task<ILiteDatabaseAsync> BeginTransactionAsync();

        /// <summary>
        /// Commit current transaction
        /// </summary>
        Task<bool> CommitAsync();

        /// <summary>
        /// Rollback current transaction
        /// </summary>
        Task<bool> RollbackAsync();
        #endregion

        #region Pragmas
        /// <summary>
        /// Get value from internal engine variables
        /// </summary>
        Task<BsonValue> PragmaAsync(string name);

        /// <summary>
        /// Set new value to internal engine variables
        /// </summary>
        Task<BsonValue> PragmaAsync(string name, BsonValue value);
        #endregion

        #region Shortcut

        /// <summary>
        /// Get all collections name inside this database.
        /// </summary>
        Task<IEnumerable<string>> GetCollectionNamesAsync();

        // TODO: CollectionExistsAsync has no unit test because upstream CollectionExists has no unit tests
        /// <summary>
        /// Checks if a collection exists on database. Collection name is case insensitive
        /// </summary>
        Task<bool> CollectionExistsAsync(string name);

        /// <summary>
        /// Drop a collection and all data + indexes
        /// </summary>
        Task<bool> DropCollectionAsync(string name);

        // TODO: RenameCollectionAsync has no unit test because upstream RenameCollection has no unit tests
        /// <summary>
        /// Rename a collection. Returns false if oldName does not exists or newName already exists
        /// </summary>
        Task<bool> RenameCollectionAsync(string oldName, string newName);

        #endregion

        #region Checkpoint/Rebuild

        /// <summary>
        /// Do database checkpoint. Copy all commited transaction from log file into datafile.
        /// </summary>
        Task CheckpointAsync();

        /// <summary>
        /// Rebuild all database to remove unused pages - reduce data file
        /// </summary>
        Task<long> RebuildAsync(RebuildOptions options = null);

        #endregion
    }
}
