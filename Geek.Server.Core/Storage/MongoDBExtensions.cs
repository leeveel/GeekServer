using MongoDB.Driver;

namespace Geek.Server
{
    public static class MongoDBExtensions
    {
        public static IMongoCollection<TDocument> GetCollection<TDocument>(this IMongoDatabase self, MongoCollectionSettings settings = null)
        {
            return self.GetCollection<TDocument>(typeof(TDocument).FullName, settings);
        }
    }
}
