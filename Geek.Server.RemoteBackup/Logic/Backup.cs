using Geek.Server.Core.Storage;
using Geek.Server.Core.Storage.DB;
using MongoDB.Driver;

namespace Geek.Server.RemoteBackup.Logic
{

    /// <summary>
    /// 1.以只读的方式打开backupdb
    /// 2.遍历SaveTimestamp表
    /// 3.与本地backupdb已经远程备份成功的时间戳做比较(有新的变化就回存)
    /// 4.将变化的state，按state类型分类
    /// 5.连上远程mongodb
    /// 6.按state类型分类批量回存
    /// 7.将远程备份成功state的时间戳写回backupdb
    /// </summary>
    internal class Backup
    {

        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// state.FullName -- data
        /// </summary>
        private readonly Dictionary<string, List<MongoState>> changedDic = new();

        private readonly string gamedbPath;
        private readonly string gamedbName;
        public Backup(string gamedbPath)
        {
            this.gamedbPath = gamedbPath;
            gamedbName = Path.GetFileName(gamedbPath);
        }

        public async Task Start()
        {
            //以只读形式打开
            var readonlyPath = Settings.InsAs<BackupSetting>().BackupDBPath
                                       + Path.DirectorySeparatorChar 
                                       + gamedbName + "_$$$";
            var gamedb = new EmbeddedDB(gamedbPath, true, readonlyPath);
            var table = gamedb.GetTable<SaveTimestamp>();
            //gamedb有可能还没进行过回存，所以不存在这个Table (直接跳过,下一轮再存)
            if(table == null)
                return;
            foreach (var item in table)
            {
                var saved = BackupDB.Get(item.Key);
                if (saved == null || saved.Timestamp < item.Timestamp)
                {
                    var rawTable = gamedb.GetRawTable(item.StateName);
                    NeedBackup(item, rawTable.Get(item.StateId));
                }
            }

            await BackupToRemote();
        }

        private void NeedBackup(SaveTimestamp st, byte[] data)
        {
            var mongoState = new MongoState()
            {
                Data = data,
                Id = st.StateId,
                Timestamp = st.Timestamp
            };
            if (changedDic.TryGetValue(st.StateName, out List<MongoState> list))
            {
                list.Add(mongoState);
            }
            else
            {
                list = new List<MongoState>();
                changedDic.Add(st.StateName, list);
                list.Add(mongoState);
            }
        }

        private async Task BackupToRemote()
        {
            foreach (var item in changedDic)
            {
                var writeList = new List<ReplaceOneModel<MongoState>>();
                foreach (var state in item.Value)
                {
                    var filter = Builders<MongoState>.Filter.Eq(MongoState.UniqueId, state.Id);
                    //& Builders<MongoState>.Filter.Lt(MongoState.TimestampName, state.Timestamp); //数据库再比较一次时间戳(保证幂等性)
                    writeList.Add(new ReplaceOneModel<MongoState>(filter, state) { IsUpsert = true });
                }
                TryCancel();
                await SaveAll(item.Key, writeList);
                TryCancel();
            }
        }

        CancellationTokenSource cancel = new CancellationTokenSource();
        public void TryCancel()
        {
            if (!BackupTask.working)
            {
                cancel.Cancel();
                cancel.Token.ThrowIfCancellationRequested();
            }
        }

        const int ONCE_SAVE_COUNT = 500;
        public static readonly ReplaceOptions REPLACE_OPTIONS = new() { IsUpsert = true };

        public static readonly BulkWriteOptions BULK_WRITE_OPTIONS = new() { IsOrdered = false };

        public async Task<bool> SaveAll(string stateName, List<ReplaceOneModel<MongoState>> writeList)
        {
            try
            {
                if(writeList.IsNullOrEmpty())
                    return true;
                var remotedb = RemoteDB.GetDB(gamedbName);
                Log.Debug($"状态回存 {stateName} count:{writeList.Count}");
                var col = remotedb.GetCollection<MongoState>(stateName);
                bool flag = true;
                for (int idx = 0; idx < writeList.Count; idx += ONCE_SAVE_COUNT)
                {
                    var list = writeList.GetRange(idx, Math.Min(ONCE_SAVE_COUNT, writeList.Count - idx));
                    var result = await col.BulkWriteAsync(list, BULK_WRITE_OPTIONS);
                    if (result.IsAcknowledged)
                    {
                        list.ForEach(model =>
                        {
                            //将远程备份成功state的时间戳写回backupdb
                            SaveTimestamp st = new()
                            {
                                StateName = stateName,
                                StateId = model.Replacement.Id,
                                Timestamp = model.Replacement.Timestamp
                            };
                            BackupDB.Set(st.Key, st);
                        });
                    }
                    else
                    {
                        Log.Error($"保存数据失败，类型:{stateName}");
                        flag = false;
                    }
                    TryCancel();
                }
                return flag;
            }
            catch (Exception e)
            {
                Log.Error($"保存数据异常，类型:{stateName}，{e}");
                return false;
            }
        }

    }
}
