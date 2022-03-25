using System;
using System.Threading.Tasks;


namespace Geek.Server
{
    public class GameLoop
    {
        static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public static async Task Enter()
        {
            //开服时间设定
            try
            {
                LOGGER.Info("regist components...");
                ComponentTools.RegistAll();

                LOGGER.Info("load hotfix module...");
                await HotfixMgr.ReloadModule("");

                LOGGER.Warn("enter game loop...");
                Console.WriteLine("enter game loop...");

                Settings.Ins.StartServerTime = DateTime.Now;

                int gcTime = 0;
                Settings.Ins.AppRunning = true;
                var random = new Random(Settings.Ins.ServerId + DateTime.Now.Millisecond);
                var gcGap = random.Next(600, 1000);//gc间隔随机
                while (Settings.Ins.AppRunning)
                {
                    //gc
                    gcTime += 1;
                    if (gcTime > gcGap)
                    {
                        gcTime = 0;
                        GC.Collect();
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("执行 Exception");
                LOGGER.Fatal(e.ToString());
            }

            Console.WriteLine("退出服务器，所有actor下线处理");
            LOGGER.Info("退出服务器，所有actor下线处理");
            await HotfixMgr.Stop();
            Console.WriteLine("下线处理完毕");
            LOGGER.Info("下线处理完毕");
        }
    }
}

