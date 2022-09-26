using Geek.Server;
using Geek.Server.Proto;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using System.Diagnostics;
using System.Text;
using Geek.Server.Common;

class Program
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    static async Task Main(string[] args)
    {
        try
        {
            AppExitHandler.Init(HandleExit);
            Console.WriteLine($"init NLog config...");
            Settings.Load<LogicSetting>("Configs/logic_config.json", ServerType.Game);
            LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
            LogManager.Configuration = new XmlLoggingConfiguration("Configs/NLog.config");
            LogManager.AutoShutdown = false;

            PolymorphicRegister.Load();
            GameLoopTask = EnterGameLoop();
            await GameLoopTask;
            if (ShutDownTask != null)
                await ShutDownTask;
        }
        catch (Exception e)
        {
            string error;
            if (Settings.AppRunning)
            {
                error = $"服务器运行时异常 e:{e}";
                Console.WriteLine(error);
            }
            else
            {
                error = $"启动服务器失败 e:{e}";
                Console.WriteLine(error);
            }

            File.WriteAllText("server_error.txt", $"{error}", Encoding.UTF8);
        }
    }

    private static volatile bool ExitCalled = false;
    private static volatile Task GameLoopTask = null;
    private static volatile Task ShutDownTask = null;

    private static async Task EnterGameLoop()
    {
        try
        {
            Log.Info($"regist comps...");
            await CompRegister.Init();
            Log.Info($"load hotfix module");
            await HotfixMgr.LoadHotfixModule();

            if (!Settings.InsAs<LogicSetting>().AllowOpen)
            {
                Log.Error($"起服逻辑执行失败，不予启动");
                return;
            }

            Log.Info($"进入游戏主循环...");
            Settings.LauchTime = DateTime.Now;
            Settings.AppRunning = true;
            TimeSpan delay = TimeSpan.FromSeconds(1);
            while (Settings.AppRunning)
            {
                await Task.Delay(delay);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"服务器执行异常，e:{e}");
            Log.Fatal(e);
        }

        Console.WriteLine($"退出服务器开始");
        await HotfixMgr.Stop();
        Console.WriteLine($"退出服务器成功");
    }

    private static void HandleExit()
    {
        if (ExitCalled)
            return;
        ExitCalled = true;
        Log.Info($"监听到退出程序消息");
        ShutDownTask = Task.Run(() =>
        {
            Settings.AppRunning = false;
            GameLoopTask?.Wait();
            LogManager.Shutdown();
            Console.WriteLine($"退出程序");
            Process.GetCurrentProcess().Kill();
        });
        ShutDownTask.Wait();
    }
}