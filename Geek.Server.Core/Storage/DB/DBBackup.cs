using NLog;
using System.IO.Compression;

namespace Core.Storage.DB
{
    public class BackupInfo
    {
        public DateTime time;
        public uint backupId;
        public ulong size;
        public uint fileNumber;
    }
    public static class DBBackup
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task BackupToZip(EmbeddedDB db, string zipBasePath)
        {
            try
            {
                await db.Flush();
                //db.InnerDB.Rebuild();
            }
            catch
            {

            }
            zipBasePath += "_backup";
            var start1 = DateTime.Now;
            if (!Directory.Exists(zipBasePath))
                Directory.CreateDirectory(zipBasePath);

            var dbName = Path.GetFileName(db.DbPath);
            var zipDirInfo = new DirectoryInfo(zipBasePath);
            var fileArr = zipDirInfo.GetFiles("*.zip");
            int fileLen = fileArr.Length;
            foreach (var f in fileArr)
            {
                var fName = Path.GetFileNameWithoutExtension(f.Name);
                var strArr = fName.Split('@');
                if (strArr.Length == 2 && strArr[0] == dbName)
                {
                    var arr2 = strArr[1].SplitToIntArray('-');
                    if (arr2.Length == 5)
                    {
                        //备份文件保留7天
                        var btime = new DateTime(arr2[0], arr2[1], arr2[2], arr2[3], arr2[4], 0);
                        if ((start1 - btime).TotalMinutes < 10)
                        {
                            Log.Error($"上次备份数据时间太近了,忽略");
                            return;
                        }
                        if ((start1 - btime).TotalDays < 7)
                            continue;
                    }
                }

                //尽量由运维删除，实在太多了再删除
                if (fileLen < 100)
                    continue;

                try
                {
                    Log.Info($"删除过期的备份数据 {f.FullName}");
                    fileLen--;
                    f.Delete();
                }
                catch (Exception e)
                {
                    Log.Error($"尝试删除老的zip备份文件失败 {f.FullName} {e.Message}");
                }
            }

            var tempPath = db.DbPath + "_zip_backup_temp";
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
            Directory.CreateDirectory(tempPath);

            var start2 = DateTime.Now;
            var tempFilePath = Path.Combine(tempPath, "data.db");

            await db.Backup(tempFilePath);

            if (!File.Exists(tempFilePath))
            {
                Log.Error($"备份数据zip出错");
                return;
            }

            var start3 = DateTime.Now;
            var fileName = dbName + $"@{DateTime.Now:yyyy-MM-dd-HH-mm}.zip";
            var targetPath = $"{zipBasePath}/{fileName}";
            try
            {
                ZipFile.CreateFromDirectory(tempPath, targetPath, CompressionLevel.Fastest, false);
            }
            catch (Exception e)
            {
                Log.Error($"备份数据zip出错，{e}");
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
            Log.Warn($"localdb 备份时间 {(DateTime.Now - start1).TotalSeconds} {(DateTime.Now - start2).TotalSeconds} {(DateTime.Now - start3).TotalSeconds}秒  {fileName}");
        }

        public static void RestoreByZip(string zipPath, string targetPath)
        {
            var tempPath = Path.GetDirectoryName(zipPath) + "_zip_restore_temp";
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            Directory.CreateDirectory(tempPath);

            try
            {
                ZipFile.ExtractToDirectory(zipPath, tempPath, false);
                File.Copy(Path.Combine(tempPath, "data.db"), targetPath, true);
                var logFile = Path.Combine(tempPath, "data-log.db");
                if (File.Exists(logFile))
                {
                    File.Copy(logFile, targetPath.Replace(".db", "-log.db"), true);
                }
            }
            catch (Exception e)
            {
                Log.Error($"备份数据zip出错，{e.Message}");
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
        }


        public static bool CheckRestore(string dbPath, string dbName)
        {
            var restorePath = Path.Combine(dbPath, dbName + "@restore.zip");
            if (File.Exists(restorePath))
            {
                try
                {
                    var dbFileName = dbName + ".db";
                    Log.Info("发现需要还原的数据，准备回档, 开始备份老db");
                    var time = DateTime.Now;
                    var oldDbPath = Path.Combine(dbPath, dbFileName);
                    var oldDbLogPath = oldDbPath.Replace(".db", "-log.db");
                    var backupPath = oldDbPath + "@restore_backup";
                    var backupLogPath = oldDbLogPath + "@restore_backup";
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    if (File.Exists(oldDbPath))
                        File.Move(oldDbPath, backupPath);

                    if (File.Exists(backupLogPath))
                        File.Delete(backupLogPath);
                    if (File.Exists(oldDbLogPath))
                        File.Move(oldDbLogPath, backupLogPath);
                    Log.Info("开始回档");
                    RestoreByZip(restorePath, Path.Combine(dbPath, dbFileName));
                    File.Delete(restorePath);
                    Log.Info($"回档完成 {(DateTime.Now - time).TotalSeconds}秒");
                    return true;
                }
                catch (Exception e)
                {
                    Log.Info("回档失败");
                    Log.Error(e.ToString());
                    return false;
                }
            }

            return true;
        }
    }
}
