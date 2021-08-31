using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class Program
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        static volatile Task gameloopTask;
        static void Main(string[] args)
        {
            Console.WriteLine("Types in this assembly:");
            foreach (Type t in typeof(Program).Assembly.GetTypes())
            {
                Console.WriteLine(t.FullName);
            }

            try
            {
                Console.WriteLine("init server...");
                AppExitHandler.Init(HandleExit);
                Settings.Load("Config/server_config.json", ServerType.Game);
                LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
                LogManager.Configuration = new XmlLoggingConfiguration("Config/NLog.config");
                LogManager.AutoShutdown = false;

                gameloopTask = EnterGameLoop();
                gameloopTask.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("start server failed. msg:" + e.Message);
                LOGGER.Error("start server failed");
                LOGGER.Error(e.ToString());
            }
        }

        static bool isExitCalled = false;
        static void HandleExit()
        {
            if (isExitCalled)
                return;
            isExitCalled = true;

            LOGGER.Info("监听到退出程序消息");
            Task.Run(() =>
            {
                Settings.Ins.AppRunning = false;
                if (gameloopTask != null)
                    gameloopTask.Wait();

                LogManager.Shutdown();
                Console.WriteLine("游戏退出");
                Process.GetCurrentProcess().Kill();
            }).Wait();
        }

        static async Task EnterGameLoop()
        {
            //设置ID规则
            ActorManager.ID_RULE = ActorID.ID_RULE;   
            LOGGER.Info("注册所有组件......");
            ComponentTools.RegistAllComps();
            var ret = await HotfixMgr.ReloadModule("");//启动游戏[hotfix工程实现一个IHotfix接口]
            if (!ret)
            {
                LOGGER.Error("起服失败");
                return;
            }
            Settings.Ins.StartServerTime = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("enter game loop 使用[ctrl+C]退出程序，不要强退，否则无法回存State");
            Console.WriteLine("压力测试请将server_config.json中IsDebug改为false");
            Console.ForegroundColor = ConsoleColor.Gray;
            LOGGER.Info("enter game loop");

            int gcTime = 0;
            Settings.Ins.AppRunning = true;
            while (Settings.Ins.AppRunning)
            {
                gcTime += 1;
                if (gcTime > 1000)//定时gc一下
                {
                    gcTime = 0;
                    GC.Collect();
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            Console.WriteLine("exit game loop 开服时长：" + (DateTime.Now - Settings.Ins.StartServerTime));
            LOGGER.Info("exit game loop 开服时长：" + (DateTime.Now - Settings.Ins.StartServerTime));
            await HotfixMgr.Stop();//退出游戏
            Console.WriteLine("exit game loop succeed");
            LOGGER.Info("exit game loop succeed");
        }
    }
}

