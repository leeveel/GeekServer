using System;
using System.Linq.Expressions;

namespace LiteDB.Async
{
    public partial class LiteCollectionAsync<T>
    {
        /// <summary>
        /// Run an include action in each document returned by Find(), FindById(), FindOne() and All() methods to load DbRef documents
        /// Returns a new Collection with this action included
        /// </summary>
        public ILiteCollectionAsync<T> Include<K>(Expression<Func<T, K>> keySelector)
        {
            return new LiteCollectionAsync<T>(UnderlyingCollection.Include(keySelector), Database);
        }

        /// <summary>
        /// Run an include action in each document returned by Find(), FindById(), FindOne() and All() methods to load DbRef documents
        /// Returns a new Collection with this action included
        /// </summary>
        public ILiteCollectionAsync<T> Include(BsonExpression keySelector)
        {
            return new LiteCollectionAsync<T>(UnderlyingCollection.Include(keySelector), Database);
        }
    }
}
