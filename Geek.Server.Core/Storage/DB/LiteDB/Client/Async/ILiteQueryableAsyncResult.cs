using System.Threading.Tasks;
using System.Collections.Generic;

namespace LiteDB.Async
{
    public interface ILiteQueryableAsyncResult<T>
    {
        /// <summary>
        /// Return a specified number of contiguous documents from start of resultset
        /// </summary>
        ILiteQueryableAsyncResult<T> Limit(int limit);

        /// <summary>
        /// Bypasses a specified number of documents in resultset and retun the remaining documents (same as Offset)
        /// </summary>
        ILiteQueryableAsyncResult<T> Skip(int offset);

        /// <summary>
        /// Bypasses a specified number of documents in resultset and retun the remaining documents (same as Skip)
        /// </summary>
        ILiteQueryableAsyncResult<T> Offset(int offset);

        /// <summary>
        /// Execute query locking collection in write mode. This is avoid any other thread change results after read document and before transaction ends
        /// </summary>
        ILiteQueryableAsyncResult<T> ForUpdate();

        //Task<BsonDocument> GetPlanAsync();
        //Task<IBsonDataReader> ExecuteReaderAsync();
        
        /// <summary>
        /// Execute query and return resultset as IEnumerable of documents
        /// </summary>
        Task<IEnumerable<BsonDocument>> ToDocumentsAsync();

        /// <summary>
        /// Execute query and return resultset as IEnumerable of T. If T is a ValueType or String, return values only (not documents)
        /// </summary>
        Task<IEnumerable<T>> ToEnumerableAsync();

        /// <summary>
        /// Execute query and return results as a List
        /// </summary>
        Task<List<T>> ToListAsync();

        /// <summary>
        /// Execute query and return results as an Array
        /// </summary>
        Task<T[]> ToArrayAsync();

        Task<int> IntoAsync(string newCollection, BsonAutoId autoId = BsonAutoId.ObjectId);


        /// <summary>
        /// Returns first document of resultset
        /// </summary>
        Task<T> FirstAsync();

        /// <summary>
        /// Returns first document of resultset or null if resultset are empty
        /// </summary>
        Task<T> FirstOrDefaultAsync();

        /// <summary>
        /// Returns the only document of resultset, and throw an exception if there not exactly one document in the sequence
        /// </summary>
        Task<T> SingleAsync();

        /// <summary>
        /// Returns the only document of resultset, or null if resultset are empty; this method throw an exception if there not exactly one document in the sequence
        /// </summary>
        Task<T> SingleOrDefaultAsync();


        /// <summary>
        /// Execute Count method in filter query
        /// </summary>
        Task<int> CountAsync();

        /// <summary>
        /// Execute Count methos in filter query
        /// </summary>
        Task<long> LongCountAsync();

        /// <summary>
        /// Returns true/false if query returns any result
        /// </summary>
        Task<bool> ExistsAsync();
    }

}
