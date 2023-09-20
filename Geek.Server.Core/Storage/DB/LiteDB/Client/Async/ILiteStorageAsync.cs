using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Async
{
    public interface ILiteStorageAsync<TFileId>
    {
        /// <summary>
        /// Find a file inside datafile and returns LiteFileInfo instance. Returns null if not found
        /// </summary>
        Task<LiteFileInfo<TFileId>> FindByIdAsync(TFileId id);

        /// <summary>
        /// Find all files that match with predicate expression.
        /// </summary>
        Task<IEnumerable<LiteFileInfo<TFileId>>> FindAsync(BsonExpression predicate);

        /// <summary>
        /// Find all files that match with predicate expression.
        /// </summary>
        Task<IEnumerable<LiteFileInfo<TFileId>>> FindAsync(string predicate, BsonDocument parameters);

        /// <summary>
        /// Find all files that match with predicate expression.
        /// </summary>
        Task<IEnumerable<LiteFileInfo<TFileId>>> FindAsync(string predicate, params BsonValue[] args);

        /// <summary>
        /// Find all files that match with predicate expression.
        /// </summary>
        Task<IEnumerable<LiteFileInfo<TFileId>>> FindAsync(Expression<Func<LiteFileInfo<TFileId>, bool>> predicate);

        /// <summary>
        /// Find all files inside file collections
        /// </summary>
        Task<IEnumerable<LiteFileInfo<TFileId>>> FindAllAsync();

        /// <summary>
        /// Returns if a file exisits in database
        /// </summary>
        Task<bool> ExistsAsync(TFileId id);

        /// <summary>
        /// Open/Create new file storage and returns linked Stream to write operations.
        /// </summary>
        Task<LiteFileStream<TFileId>> OpenWriteAsync(TFileId id, string filename, BsonDocument metadata = null);

        /// <summary>
        /// Upload a file based on stream data
        /// </summary>
        Task<LiteFileInfo<TFileId>> UploadAsync(TFileId id, string filename, Stream stream, BsonDocument metadata = null);

        /// <summary>
        /// Upload a file based on file system data
        /// </summary>
        Task<LiteFileInfo<TFileId>> UploadAsync(TFileId id, string filename);

        /// <summary>
        /// Update metadata on a file. File must exist.
        /// </summary>
        Task<bool> SetMetadataAsync(TFileId id, BsonDocument metadata);

        /// <summary>
        /// Load data inside storage and returns as Stream
        /// </summary>
        Task<LiteFileStream<TFileId>> OpenReadAsync(TFileId id);

        /// <summary>
        /// Copy all file content to a steam
        /// </summary>
        Task<LiteFileInfo<TFileId>> DownloadAsync(TFileId id, Stream stream);

        /// <summary>
        /// Copy all file content to a file
        /// </summary>
        Task<LiteFileInfo<TFileId>> DownloadAsync(TFileId id, string filename, bool overwritten);

        /// <summary>
        /// Delete a file inside datafile and all metadata related
        /// </summary>
        Task<bool> DeleteAsync(TFileId id);
    }
}
