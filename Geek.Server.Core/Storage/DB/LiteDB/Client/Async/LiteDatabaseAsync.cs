using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using LiteDB.Engine;
using litedbasync.Utils;
using NLog;
using System.Diagnostics;

namespace LiteDB.Async
{
    public class LiteDatabaseAsync : ILiteDatabaseAsync
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private readonly Thread _backgroundThread;
        private readonly SemaphoreSlim _newTaskArrived = new(initialCount: 0, maxCount: int.MaxValue);
        private readonly CancellationTokenSource _shouldTerminate = new();
        private readonly ConcurrentQueue<Action> _queue = new();
        private readonly bool _disposeOfWrappedDatabase = true;
        private static readonly DefaultDictionary<ILiteDatabase, int> _wrappedDatabases = new();
        private static readonly object _hashSetLock = new();
        private bool _isClosedTransaction = false;

        /// <summary>
        /// Starts LiteDB database using a connection string for file system database
        /// </summary>
        public LiteDatabaseAsync(string connectionString, BsonMapper mapper = null)
            : this(new ConnectionString(connectionString), mapper)
        {
        }

        /// <summary>
        /// Starts LiteDB database using a connection string for file system database
        /// </summary>
        public LiteDatabaseAsync(ConnectionString connectionString, BsonMapper mapper = null)
        {
            _connectionString = connectionString;
            UnderlyingDatabase = new LiteDatabase(connectionString, mapper);
            RecordUnderlyingDatabaseInMap(UnderlyingDatabase);
            _backgroundThread = new Thread(BackgroundLoop)
            {
                Priority = ThreadPriority.Highest
            };
            _backgroundThread.Start();
        }

        internal void VerifyNoClosedTransaction()
        {
            if (_isClosedTransaction)
            {
                throw new LiteAsyncException("Transaction Closed, no further writes are allowed.");
            }
        }

        /// <summary>
        /// Starts LiteDB database using a generic Stream implementation (mostly MemoryStrem).
        /// Use another MemoryStrem as LOG file.
        /// </summary>
        /// <param name="stream">DataStream reference </param>
        /// <param name="mapper">BsonMapper mapper reference</param>
        /// <param name="logStream">LogStream reference </param>
        public LiteDatabaseAsync(Stream stream, BsonMapper mapper = null, Stream logStream = null)
        {
            UnderlyingDatabase = new LiteDatabase(stream, mapper, logStream);
            RecordUnderlyingDatabaseInMap(UnderlyingDatabase);
            _backgroundThread = new Thread(BackgroundLoop);
            _backgroundThread.Start();
        }

        /// <summary>
        /// Starts LiteDB database wrapping the passed in LiteDatabase instance
        /// </summary>
        /// <param name="wrappedDatabase">ILiteDatabase reference </param>
        /// <param name="bool">disposeOfWrappedDatabase iff true dispose of the wrappedDatabase when this object is disposed</param>
        public LiteDatabaseAsync(ILiteDatabase wrappedDatabase, bool disposeOfWrappedDatabase = true)
        {
            UnderlyingDatabase = wrappedDatabase ?? throw new ArgumentNullException($"{nameof(wrappedDatabase)} cannot be null");
            RecordUnderlyingDatabaseInMap(UnderlyingDatabase);
            _backgroundThread = new Thread(BackgroundLoop);
            _backgroundThread.Start();
            _disposeOfWrappedDatabase = disposeOfWrappedDatabase;
        }

        private static void RecordUnderlyingDatabaseInMap(ILiteDatabase underlyingDatabase)
        {
            lock (_hashSetLock)
            {
                if (_wrappedDatabases.ContainsKey(underlyingDatabase))
                {
                    throw new LiteAsyncException("You can only have one LiteDatabaseAsync per LiteDatabase.");
                }
                _wrappedDatabases[underlyingDatabase] = _wrappedDatabases[underlyingDatabase] + 1;
            }
        }

        /// <summary>
        /// This private constructor starts a transaction on the same UnderlyingDatabase as sourceDatabaseAsync. Private because it should
        /// only be called by the BeginTransactionAsync function
        /// </summary>
        /// <param name="sourceDatabaseAsync"></param>
        private LiteDatabaseAsync(ILiteDatabaseAsync sourceDatabaseAsync, bool disposeOfWrappedDatabase)
        {
            UnderlyingDatabase = sourceDatabaseAsync.UnderlyingDatabase;
            lock (_hashSetLock)
            {
                _wrappedDatabases[UnderlyingDatabase] = _wrappedDatabases[UnderlyingDatabase] + 1;
            }
            _backgroundThread = new Thread(BackgroundLoop);
            _backgroundThread.Start();
            _disposeOfWrappedDatabase = disposeOfWrappedDatabase;
        }

        /// <summary>
        /// Gets the underlying <see cref="ILiteDatabase"/>. Useful to access various operations not exposed by <see cref="ILiteDatabaseAsync"/>
        /// </summary>
        public ILiteDatabase UnderlyingDatabase { get; }


        public bool UtcDate
        {
            get => UnderlyingDatabase.UtcDate;
            set => UnderlyingDatabase.UtcDate = value;
        }

        public int CheckpointSize
        {
            get => UnderlyingDatabase.CheckpointSize;
            set => UnderlyingDatabase.CheckpointSize = value;
        }

        public int UserVersion
        {
            get => UnderlyingDatabase.UserVersion;
            set => UnderlyingDatabase.UserVersion = value;
        }

        public TimeSpan Timeout
        {
            get => UnderlyingDatabase.Timeout;
            set => UnderlyingDatabase.Timeout = value;
        }

        public Collation Collation
        {
            get => UnderlyingDatabase.Collation;
        }

        public long LimitSize
        {
            get => UnderlyingDatabase.LimitSize;
            set => UnderlyingDatabase.LimitSize = value;
        }
         
        private void BackgroundLoop()
        {
            var terminationToken = _shouldTerminate.Token;

            try
            {
                while (!terminationToken.IsCancellationRequested)
                {
                    _newTaskArrived.Wait(terminationToken); 

                    if (!_queue.TryDequeue(out var function)) continue;
 
                    try
                    {
                        function();
                    }
                    catch (Exception e)
                    {
                        LOGGER.Error(e);
                    } 
                } 
            }
            catch (OperationCanceledException) when (terminationToken.IsCancellationRequested)
            {
                // it's OK, we're exiting
            }
        }

        internal Task<T> EnqueueAsync<T>(LiteAsyncDelegate<T> function)
        {
            VerifyNoClosedTransaction();
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Function()
            {
                try
                {
                    tcs.SetResult(function());
                }
                catch (Exception ex)
                {
                    tcs.SetException(new LiteAsyncException("LiteDB encounter an error. Details in the inner exception.", ex));
                }
            }

            _queue.Enqueue(Function);
            _newTaskArrived.Release();
            return tcs.Task;
        }

        #region Collections
        /// <summary>
        /// Get a collection using a name based on typeof(T).Name (BsonMapper.ResolveCollectionName function)
        /// </summary>
        public ILiteCollectionAsync<T> GetCollection<T>()
        {
            return this.GetCollection<T>(null);
        }

        /// <summary>
        /// Get a collection using a entity class as strong typed document. If collection does not exits, create a new one.
        /// </summary>
        /// <param name="name">Collection name (case insensitive)</param>
        public ILiteCollectionAsync<T> GetCollection<T>(string name)
        {
            return new LiteCollectionAsync<T>(UnderlyingDatabase.GetCollection<T>(name), this);
        }

        /// <summary>
        /// Get a collection using a generic BsonDocument. If collection does not exits, create a new one.
        /// </summary>
        /// <param name="name">Collection name (case insensitive)</param>
        /// <param name="autoId">Define autoId data type (when document contains no _id field)</param>
        public ILiteCollectionAsync<BsonDocument> GetCollection(string name, BsonAutoId autoId = BsonAutoId.ObjectId)
        {
            return new LiteCollectionAsync<BsonDocument>(UnderlyingDatabase.GetCollection(name, autoId), this);
        }
        #endregion

        #region FileStorage

        private ILiteStorageAsync<string> _fs = null;
        private ConnectionString _connectionString = null;

        /// <summary>
        /// Returns a special collection for storage files/stream inside datafile. Use _files and _chunks collection names. FileId is implemented as string. Use "GetStorage" for custom options
        /// </summary>
        public ILiteStorageAsync<string> FileStorage
        {
            get { return _fs ?? (_fs = this.GetStorage<string>()); }
        }

        /// <summary>
        /// Get new instance of Storage using custom FileId type, custom "_files" collection name and custom "_chunks" collection. LiteDB support multiples file storages (using different files/chunks collection names)
        /// </summary>
        public ILiteStorageAsync<TFileId> GetStorage<TFileId>(string filesCollection = "_files", string chunksCollection = "_chunks")
        {
            return new LiteStorageAsync<TFileId>(this, UnderlyingDatabase, filesCollection, chunksCollection);
        }

        #endregion

        #region Transactions
        /// <summary>
        /// Commit current transaction
        /// </summary>
        public async Task<bool> CommitAsync()
        {
            var result = await EnqueueAsync(
                () => UnderlyingDatabase.Commit());
            _isClosedTransaction = true;
            return result;
        }

        /// <summary>
        /// Rollback current transaction
        /// </summary>
        public Task<bool> RollbackAsync()
        {
            var result = EnqueueAsync(
                () => UnderlyingDatabase.Rollback());
            _isClosedTransaction = true;
            return result;
        }

        /// <summary>
        /// Initialize a new transaction. Transaction are created "per-thread". There is only one single transaction per thread.
        /// Return true if transaction was created or false if current thread already in a transaction.
        /// </summary>
        public async Task<ILiteDatabaseAsync> BeginTransactionAsync()
        {
            // Make a new database
            var result = new LiteDatabaseAsync(this, _disposeOfWrappedDatabase);
            // Begin transaction on it
            await result.EnqueueAsync<bool>(() =>
                UnderlyingDatabase.BeginTrans()
            );
            // Return once the new database is in transaction mode
            return result;
        }

        #endregion

        #region Pragmas
        /// <summary>
        /// Get value from internal engine variables
        /// </summary>
        public Task<BsonValue> PragmaAsync(string name)
        {
            return EnqueueAsync(
                () => UnderlyingDatabase.Pragma(name));
        }

        /// <summary>
        /// Set new value to internal engine variables
        /// </summary>
        public Task<BsonValue> PragmaAsync(string name, BsonValue value)
        {
            return EnqueueAsync(
                () => UnderlyingDatabase.Pragma(name, value));
        }
        #endregion

        #region Shortcut

        /// <summary>
        /// Get all collections name inside this database.
        /// </summary>
        public Task<IEnumerable<string>> GetCollectionNamesAsync()
        {
            return EnqueueAsync(
                () => UnderlyingDatabase.GetCollectionNames());
        }

        /// <summary>
        /// Checks if a collection exists on database. Collection name is case insensitive
        /// </summary>
        public Task<bool> CollectionExistsAsync(string name)
        {
            return EnqueueAsync(
                () => UnderlyingDatabase.CollectionExists(name));
        }

        /// <summary>
        /// Drop a collection and all data + indexes
        /// </summary>
        public Task<bool> DropCollectionAsync(string name)
        {
            return EnqueueAsync(
                () => UnderlyingDatabase.DropCollection(name));
        }

        /// <summary>
        /// Rename a collection. Returns false if oldName does not exists or newName already exists
        /// </summary>
        public Task<bool> RenameCollectionAsync(string oldName, string newName)
        {
            return EnqueueAsync(
                () => UnderlyingDatabase.RenameCollection(oldName, newName));
        }

        #endregion

        #region Checkpoint/Rebuild

        /// <summary>
        /// Do database checkpoint. Copy all commited transaction from log file into datafile.
        /// </summary>
        public Task CheckpointAsync()
        {
            return EnqueueAsync(() =>
            {
                UnderlyingDatabase.Checkpoint();
                return true;
            });
        }

        /// <summary>
        /// Rebuild all database to remove unused pages - reduce data file
        /// </summary>
        public Task<long> RebuildAsync(RebuildOptions options = null)
        {
            return EnqueueAsync(
                () => UnderlyingDatabase.Rebuild(options));
        }

        #endregion
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (_shouldTerminate)
                using (_newTaskArrived)
                {
                    _shouldTerminate.Cancel();

                    // give the thread 5 seconds to exit... must not block forever here
                    _backgroundThread.Join(TimeSpan.FromSeconds(10));
                }
                int newCount;
                lock (_hashSetLock)
                {
                    newCount = _wrappedDatabases[UnderlyingDatabase] - 1;
                    if (newCount == 0)
                    {
                        _wrappedDatabases.Remove(UnderlyingDatabase);
                    }
                    else
                    {
                        _wrappedDatabases[UnderlyingDatabase] = newCount;
                    }
                }
                if (_disposeOfWrappedDatabase && newCount == 0)
                {
                    UnderlyingDatabase.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Backup(string targetPath)
        {
            var result = await EnqueueAsync(
                () =>
                {
                    UnderlyingDatabase.Engine.Backup(targetPath);
                    return Task.CompletedTask;
                });
        }
    }
}
