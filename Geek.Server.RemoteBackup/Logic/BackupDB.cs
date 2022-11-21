using Geek.Server.Core.Storage;
using Geek.Server.Core.Storage.DB;
using NLog;

namespace Geek.Server.RemoteBackup.Logic
{
    internal class BackupDB
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static EmbeddedDB DB { get; private set; }

        public static Table<SaveTimestamp> BackupTable { get; private set; }

        public static void Open()
        {
            try
            {
                var backSetting = Settings.InsAs<BackupSetting>();
                DB = new EmbeddedDB(backSetting.BackupDBPath + backSetting.BackupDBName);
                BackupTable = DB.GetTable<SaveTimestamp>();
            }
            catch (Exception e)
            {
                Log.Fatal($"本地数据库连接失败:{e}");
                throw;
            }
        }

        public static void Close()
        {
            DB.Close();
        }

        public static SaveTimestamp Get(string key)
        {
            try
            {
                return BackupTable.Get(key);
            }
            catch (Exception e)
            {
                Log.Fatal(e.ToString());
                return default;
            }
        }

        public static void Set(string key, SaveTimestamp val)
        {
            try
            {
                BackupTable.Set(key, val);
            }
            catch (Exception e)
            {
                Log.Fatal(e.ToString());
            }
        }

    }
}
