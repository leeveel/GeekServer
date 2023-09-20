using System.Threading.Tasks;
using System.Collections.Generic;

namespace LiteDB.Async
{
    public partial class LiteCollectionAsync<T>
    {
        /// <summary>
        /// Insert a new entity to this collection. Document Id must be a new value in collection - Returns document Id
        /// </summary>
        public Task<BsonValue> InsertAsync(T entity)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Insert(entity));
        }

        /// <summary>
        /// Insert a new document to this collection using passed id value.
        /// </summary>
        public Task InsertAsync(BsonValue id, T entity)
        {
            return Database.EnqueueAsync(() => {
                UnderlyingCollection.Insert(id, entity);
                return true;
            });
        }

        /// <summary>
        /// Insert an array of new documents to this collection. Document Id must be a new value in collection. Can be set buffer size to commit at each N documents
        /// </summary>
        public Task<int> InsertAsync(IEnumerable<T> entities)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.Insert(entities));
        }

        /// <summary>
        /// Implements bulk insert documents in a collection. Usefull when need lots of documents.
        /// </summary>
        public Task<int> InsertBulkAsync(IEnumerable<T> entities, int batchSize = 5000)
        {
            return Database.EnqueueAsync(
                () => UnderlyingCollection.InsertBulk(entities, batchSize));
        }
    }
}
