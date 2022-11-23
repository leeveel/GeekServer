using Geek.Server.Core.Utils;

namespace Geek.Server.RemoteBackup.Logic
{
    internal class BackupTask
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private static Task LoopTask;
        public static volatile bool working = false;
        public static void Start()
        {
            working = true;
            LoopTask = Task.Run(Loop);
            Log.Info($"初始化备份任务完成");
        }

        private static async Task Loop()
        {
            var nextSaveTime = NextSaveTime();
            var ONCE_DELAY = TimeSpan.FromMilliseconds(200);
            while (working)
            {
                Log.Info($"下次定时回存时间 {nextSaveTime}");
                while (DateTime.Now < nextSaveTime && working)
                {
                    await Task.Delay(ONCE_DELAY);
                }
                if (!working)
                    break;
                var startTime = DateTime.Now;

                await StartNewTurn();

                var cost = (DateTime.Now - startTime).TotalMilliseconds;
                Log.Info($"定时回存完成 耗时: {cost:f4}ms");
                nextSaveTime = NextSaveTime(cost);
            }
        }

        /// <summary>
        /// 开始新的一轮备份
        /// </summary>
        /// <returns></returns>
        private static async Task StartNewTurn()
        {
            try
            {
                var paths = GetAllDatabasePath();
                foreach (var path in paths)
                {
                    if (!working)
                        break;
                    var backup = new Backup(path);
                    var startTime = DateTime.Now;
                    await backup.Start();
                    var cost = (DateTime.Now - startTime).TotalMilliseconds;
                    Log.Info($"{path} 定时回存完成 耗时: {cost:f4}ms");
                    await Task.Delay(3000);
                }
            }
            catch (System.OperationCanceledException)
            {
                Log.Error("监听到关闭进程事件，取消正在执行的回存操作");
            }
            catch (Exception e)
            {
                Log.Error($"StartNewTurn Throw Exception:{e}");
            }
        }

        public static List<string> GetAllDatabasePath()
        {
            string basePath = Settings.InsAs<BackupSetting>().DBRootPath;
            var paths = Directory.GetDirectories(basePath);
            List<string> result = new List<string>();
            foreach (var path in paths)
            {
                if (path.Contains(Settings.LocalDBPrefix) && !path.Contains("_$$$"))
                {
                    result.Add(path);
                }
            }
            return result;
        }

        //最小回存间隔时间(3分钟)
        const int MIN_SAVE_INTERVAL_IN_MilliSECONDS = 180_000;
        private static DateTime NextSaveTime(double cost=-1)
        {
            //如果是刚启动进程，等待10秒之后再开始备份
            if (cost < 0)
                return DateTime.Now.AddSeconds(10);

            if (cost < MIN_SAVE_INTERVAL_IN_MilliSECONDS)
                return DateTime.Now.AddMilliseconds(MIN_SAVE_INTERVAL_IN_MilliSECONDS - cost);

            return DateTime.Now;
        }

        public static async Task Stop()
        {
            working = false;
            await LoopTask;
            BackupDB.Close();
            Log.Info($"停止备份任务完成");
        }

    }
}
