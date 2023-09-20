using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Geek.Server.Core.Storage
{
    public static class FileBackup
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public static FileBackupStatus CheckRestoreFromFile()
        {
            var folder = Environment.CurrentDirectory + "/../State/";
            if (Directory.Exists(folder))
            {
                LOGGER.Warn("need restore...");

                try
                {
                    var curDataBase = GameDB.As<MongoDBConnection>().CurDB;
                    var root = new DirectoryInfo(folder);
                    foreach (var dir in root.GetDirectories())
                    {
                        foreach (var jsonDir in dir.GetDirectories())
                        {
                            foreach (var file in jsonDir.GetFiles())
                            {
                                var batchList = new List<ReplaceOneModel<BsonDocument>>();
                                var col = curDataBase.GetCollection<BsonDocument>(jsonDir.Name);
                                var fileStr = File.ReadAllText(file.FullName);
                                BsonDocument bsonElements = BsonDocument.Parse(fileStr);
                                var filter = Builders<BsonDocument>.Filter.Eq(CacheState.UniqueId, bsonElements.GetValue(CacheState.UniqueId));
                                var ret = new ReplaceOneModel<BsonDocument>(filter, bsonElements) { IsUpsert = true };
                                batchList.Add(ret);
                                //保存数据
                                var result = col.BulkWrite(batchList);
                                if (!result.IsAcknowledged)
                                {
                                    LOGGER.Warn($"restore {jsonDir.Name} fail");
                                    return FileBackupStatus.StoreToDbFailed;
                                }
                            }
                        }
                    }
                    //删除目录文件夹
                    var destDir = Environment.CurrentDirectory + $"/../State_Back";
                    destDir.CreateAsDirectory();
                    Directory.Move(folder, $"{destDir}/{DateTime.Now:yyyy-MM-dd-HH-mm}");

                    return FileBackupStatus.StoreToDbSuccess;
                }
                catch (Exception e)
                {
                    LOGGER.Fatal(e.ToString());
                    //回存数据失败 不予启服
                    return FileBackupStatus.StoreToDbFailed;
                }
            }
            else
            {
                return FileBackupStatus.NoFile;
            }
        }

        internal static async Task SaveToFile(List<long> ids, List<ReplaceOneModel<MongoDB.Bson.BsonDocument>> docs, string stateName)
        {
            var folder = Environment.CurrentDirectory + $"/../State/{DateTime.Now:yyyy-MM-dd-HH-mm}/{stateName}/";
            folder.CreateAsDirectory();
            for(int i=0;i<ids.Count;i++)
            {
                var id = ids[i];
                var doc = docs[i];
                var str = doc.ToString();
                var path = folder + id + ".json";
                await File.WriteAllTextAsync(path, str);
            }
        }
    }


    public enum FileBackupStatus
    {
        NoFile,
        StoreToDbFailed,
        StoreToDbSuccess,
    }
}
