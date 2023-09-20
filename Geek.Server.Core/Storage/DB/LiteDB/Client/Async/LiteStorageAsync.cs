using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LiteDB.Async
{
    public class LiteStorageAsync<TFileId> : ILiteStorageAsync<TFileId>
    {
        private readonly ILiteStorage<TFileId> _wrappedStorage;
        private readonly LiteDatabaseAsync _liteDatabaseAsync;

        public LiteStorageAsync(LiteDatabaseAsync liteDatabaseAsync, ILiteDatabase liteDb, string filesCollection = "_files", string chunksCollection = "_chunks")
        {
            _wrappedStorage = new LiteStorage<TFileId>(liteDb, filesCollection, chunksCollection);
            _liteDatabaseAsync = liteDatabaseAsync;
        }

        /// <summary>
        /// Find a file inside datafile and returns LiteFileInfo instance. Returns null if not found
        /// </summary>
        public Task<LiteFileInfo<TFileId>> FindByIdAsync(TFileId id)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.FindById(id));
        }

        /// <summary>
        /// Find all files that match with predicate expression.
        /// </summary>
        public Task<IEnumerable<LiteFileInfo<TFileId>>> FindAsync(BsonExpression predicate)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.Find(predicate));
        }

        /// <summary>
        /// Find all files that match with predicate expression.
        /// </summary>
        public Task<IEnumerable<LiteFileInfo<TFileId>>> FindAsync(string predicate, BsonDocument parameters)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.Find(predicate, parameters));
        }

        /// <summary>
        /// Find all files that match with predicate expression.
        /// </summary>
        public Task<IEnumerable<LiteFileInfo<TFileId>>> FindAsync(string predicate, params BsonValue[] args)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.Find(predicate, args));
        }

        /// <summary>
        /// Find all files that match with predicate expression.
        /// </summary>
        public Task<IEnumerable<LiteFileInfo<TFileId>>> FindAsync(Expression<Func<LiteFileInfo<TFileId>, bool>> predicate)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.Find(predicate));
        }

        /// <summary>
        /// Find all files inside file collections
        /// </summary>
        public Task<IEnumerable<LiteFileInfo<TFileId>>> FindAllAsync()
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.FindAll());
        }

        /// <summary>
        /// Returns if a file exisits in database
        /// </summary>
        public Task<bool> ExistsAsync(TFileId id)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.Exists(id));
        }

        /// <summary>
        /// Open/Create new file storage and returns linked Stream to write operations.
        /// </summary>
        public Task<LiteFileStream<TFileId>> OpenWriteAsync(TFileId id, string filename, BsonDocument metadata = null)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.OpenWrite(id, filename, metadata));
        }

        /// <summary>
        /// Upload a file based on stream data
        /// </summary>
        public Task<LiteFileInfo<TFileId>> UploadAsync(TFileId id, string filename, Stream stream, BsonDocument metadata = null)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.Upload(id, filename, stream, metadata));
        }

        /// <summary>
        /// Upload a file based on file system data
        /// </summary>
        public Task<LiteFileInfo<TFileId>> UploadAsync(TFileId id, string filename)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.Upload(id, filename));
        }

        /// <summary>
        /// Update metadata on a file. File must exist.
        /// </summary>
        public Task<bool> SetMetadataAsync(TFileId id, BsonDocument metadata)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.SetMetadata(id, metadata));
        }

        /// <summary>
        /// Load data inside storage and returns as Stream
        /// </summary>
        public Task<LiteFileStream<TFileId>> OpenReadAsync(TFileId id)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.OpenRead(id));
        }

        /// <summary>
        /// Copy all file content to a steam
        /// </summary>
        public Task<LiteFileInfo<TFileId>> DownloadAsync(TFileId id, Stream stream)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.Download(id, stream));
        }

        /// <summary>
        /// Copy all file content to a file
        /// </summary>
        public Task<LiteFileInfo<TFileId>> DownloadAsync(TFileId id, string filename, bool overwritten)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.Download(id, filename, overwritten));
        }

        /// <summary>
        /// Delete a file inside datafile and all metadata related
        /// </summary>
        public Task<bool> DeleteAsync(TFileId id)
        {
            return _liteDatabaseAsync.EnqueueAsync(
                () => _wrappedStorage.Delete(id));
        }
    }
}
