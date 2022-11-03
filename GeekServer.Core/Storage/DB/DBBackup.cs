using System.IO.Compression;
using System.Runtime.InteropServices;
using NLog;
using RocksDbSharp;

namespace Geek.Server
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
        public static String Backup(EmbeddedDB db, string path, uint backupsKeepNum = 12)
        {
            string err = null;
            using (var pathSafe = new RocksSafePath(path))
            {
                var optionPtr = Native.Instance.rocksdb_backup_engine_options_create(pathSafe.Handle);
                var bePtr = Native.Instance.rocksdb_backup_engine_open_opts(optionPtr, Env.CreateDefaultEnv().Handle);
                Native.Instance.rocksdb_backup_engine_create_new_backup_flush(bePtr, db.InnerDB.Handle, true, out IntPtr errPtr);

                if (errPtr != IntPtr.Zero)
                {
                    err = Marshal.PtrToStringAnsi(errPtr);
                    Native.Instance.rocksdb_free(errPtr);
                }

                Native.Instance.rocksdb_backup_engine_purge_old_backups(bePtr, backupsKeepNum, out var errPtr1);
                Native.Instance.rocksdb_free(errPtr1);

                Native.Instance.rocksdb_backup_engine_options_destroy(optionPtr);
                Native.Instance.rocksdb_backup_engine_close(bePtr);
            }
            return err;
        }

        public static string BackupToZip(EmbeddedDB db, string zipBasePath)
        {
            if (!Directory.Exists(zipBasePath))
            {
                Directory.CreateDirectory(zipBasePath);
            }
            var zipDirInfo = new DirectoryInfo(zipBasePath);
            foreach (var f in zipDirInfo.GetFiles("*.zip"))
            {
                try
                {
                    f.Delete();
                }
                catch (Exception e)
                {
                    Log.Info($"尝试删除老的zip备份文件失败{e.Message}");
                }
            }

            var tempPath = db.DbPath + "_zip_backup_temp";
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            Directory.CreateDirectory(tempPath);

            var err = Backup(db, tempPath, 1);
            if (err != null)
            {
                Log.Error($"备份数据zip出错，{err}");
                return err;
            }
            //var fileName = Path.GetFileName(db.dbPath) + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".zip";
            var fileName = Path.GetFileName(db.DbPath) + "_backup.zip";
            var targetPath = $"{zipBasePath}/{fileName}";
            try
            {
                ZipFile.CreateFromDirectory(tempPath, targetPath, CompressionLevel.SmallestSize, false);
            }
            catch (Exception e)
            {
                err = e.Message;
                Log.Error($"备份数据zip出错，{err}");
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
            return err;
        }


        public static List<BackupInfo> GetBackupInfos(string path)
        {
            var result = new List<BackupInfo>();
            using (var pathSafe = new RocksSafePath(path))
            {
                var optionPtr = Native.Instance.rocksdb_backup_engine_options_create(pathSafe.Handle);
                var bePtr = Native.Instance.rocksdb_backup_engine_open_opts(optionPtr, Env.CreateDefaultEnv().Handle);

                var infosPtr = Native.Instance.rocksdb_backup_engine_get_backup_info(bePtr);
                var count = Native.Instance.rocksdb_backup_engine_info_count(infosPtr);
                for (int i = 0; i < count; i++)
                {
                    var info = new BackupInfo();
                    result.Add(info);
                    info.backupId = Native.Instance.rocksdb_backup_engine_info_backup_id(infosPtr, i);
                    info.size = Native.Instance.rocksdb_backup_engine_info_size(infosPtr, i);
                    info.fileNumber = Native.Instance.rocksdb_backup_engine_info_number_files(infosPtr, i);
                    var timestamp = Native.Instance.rocksdb_backup_engine_info_timestamp(infosPtr, i);
                    System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
                    info.time = startTime.AddSeconds(timestamp);
                }
                Native.Instance.rocksdb_backup_engine_info_destroy(infosPtr);
                Native.Instance.rocksdb_backup_engine_options_destroy(optionPtr);
                Native.Instance.rocksdb_backup_engine_close(bePtr);
            }
            return result;
        }

        public static string Restore(string backupPath, string targetPath, uint backupId = 0)
        {
            string err = null;
            using (var pathSafe = new RocksSafePath(backupPath))
            {
                var optionPtr = Native.Instance.rocksdb_backup_engine_options_create(pathSafe.Handle);
                var bePtr = Native.Instance.rocksdb_backup_engine_open_opts(optionPtr, Env.CreateDefaultEnv().Handle);
                var restorOptionPtr = Native.Instance.rocksdb_restore_options_create();

                if (backupId == 0)
                {
                    Native.Instance.rocksdb_backup_engine_restore_db_from_latest_backup(bePtr, targetPath, targetPath, restorOptionPtr, out var errPtr);
                    if (errPtr != IntPtr.Zero)
                    {
                        err = Marshal.PtrToStringAnsi(errPtr);
                        Native.Instance.rocksdb_free(errPtr);
                    }
                }
                else
                {
                    Native.Instance.rocksdb_backup_engine_restore_db_from_backup(bePtr, targetPath, targetPath, restorOptionPtr, backupId, out var errPtr);
                    if (errPtr != IntPtr.Zero)
                    {
                        err = Marshal.PtrToStringAnsi(errPtr);
                        Native.Instance.rocksdb_free(errPtr);
                    }
                }

                Native.Instance.rocksdb_restore_options_destroy(restorOptionPtr);
                Native.Instance.rocksdb_backup_engine_options_destroy(optionPtr);
                Native.Instance.rocksdb_backup_engine_close(bePtr);
            }
            return err;
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
                Restore(tempPath, targetPath);
            }
            catch (Exception e)
            {
                Log.Error($"备份数据zip出错，{e.Message}");
            }
            finally
            {
                //Directory.Delete(tempPath, true);
            }
        }
    }
}