using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Geek.Server
{
    public class FileBackUp
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public static bool CheckRestoreFromFile()
        {
            var folder = Environment.CurrentDirectory + "/../State/";
            if (Directory.Exists(folder))
            {
                LOGGER.Warn("need restore...");

                if(!Settings.Ins.restoreFromFile)
                {
                    LOGGER.Error("can not restore from file, please modify config file named 'server_config.json'...");
                    return false;
                }
                try
                {
                    LOGGER.Warn($"connect mongo {Settings.Ins.mongoDB} {Settings.Ins.mongoUrl}...");
                    MongoDBConnection.Singleton.Connect(Settings.Ins.mongoDB, Settings.Ins.mongoUrl);
                    var curDataBase = MongoDBConnection.Singleton.CurDateBase;
                    DirectoryInfo root = new DirectoryInfo(folder);
                    foreach (var dir in root.GetDirectories())
                    {
                        foreach (var jsonDir in dir.GetDirectories())
                        {
                            foreach (var file in jsonDir.GetFiles())
                            {
                                List<ReplaceOneModel<BsonDocument>> batchList = new List<ReplaceOneModel<BsonDocument>>();
                                var col = curDataBase.GetCollection<BsonDocument>(jsonDir.Name);
                                var fileStr = File.ReadAllText(file.FullName);
                                BsonDocument bsonElements = BsonDocument.Parse(fileStr);
                                var filter = Builders<BsonDocument>.Filter.Eq(MongoField.Id, bsonElements.GetValue("_id"));
                                var ret = new ReplaceOneModel<BsonDocument>(filter, bsonElements) { IsUpsert = true };
                                batchList.Add(ret);
                                //保存数据
                                var result = col.BulkWrite(batchList);
                                if (!result.IsAcknowledged)
                                {
                                    LOGGER.Warn($"restore {jsonDir.Name} fail");
                                    return false;
                                }
                            }
                        }
                    }
                    //删除目录文件夹
                    var destDir = Environment.CurrentDirectory + $"/../State_Back";
                    destDir.CreateAsDirectory();
                    Directory.Move(folder, $"{destDir}/{DateTime.Now:yyyy-MM-dd-HH-mm}");
                }
                catch (Exception e)
                {
                    LOGGER.Fatal(e.ToString());
                    //回存数据失败 不予启服
                    return false;
                }
            }
            else
            {
                LOGGER.Warn("not need restore...");
            }
            return true;
        }

        public static void StoreToFile<T>(List<ReplaceOneModel<T>> list) where T : DBState, new()
        {
            var folder = Environment.CurrentDirectory + $"/../State/{DateTime.Now:yyyy-MM-dd-HH-mm}/{typeof(T).FullName}/";
            folder.CreateAsDirectory();
            foreach (var one in list)
            {
                var state = one.Replacement;
                Newtonsoft.Json.JsonConvert.SerializeObject(state);
                //var bson = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(state.ToBson());
                //var str = bson.ToJson();
                var str = Newtonsoft.Json.JsonConvert.SerializeObject(state);
                var path = folder + state.Id + ".json";
                System.IO.File.WriteAllText(path, str);
            }
        }
    }
}
