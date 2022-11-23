using Geek.Server.Core.Utils;
using MongoDB.Driver;
using NLog;

namespace Geek.Server.RemoteBackup.Logic
{
    public static class RemoteDB
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static MongoClient Client { get; private set; }

        public static void Connect()
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString(Settings.MongoUrl);
                Client = new MongoClient(settings);
                Client.ListDatabaseNames();
                Log.Info($"初始化MongoDB服务完成 Url:{Settings.MongoUrl}");
            }
            catch (Exception e)
            {
                Log.Error($"初始化MongoDB服务失败 Url:{Settings.MongoUrl}, {e}");
                throw;
            }
        }

        public static IMongoDatabase GetDB(string dbname)
        {
            return Client.GetDatabase(dbname);
        }


    }
}
