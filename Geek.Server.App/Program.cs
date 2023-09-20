using Geek.Server.App.Common;
using Geek.Server.Core.Utils;
using NLog;
using System.Diagnostics;
using System.Text;

namespace Geek.Server.App
{
    class Program
    { 
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(); 

        private static volatile bool ExitCalled = false;
        private static volatile Task GameLoopTask = null;
        private static volatile Task ShutDownTask = null;

        static async Task Main(string[] args)
        {
            try
            {
                AppExitHandler.Init(HandleExit);
                GameLoopTask = AppStartUp.Enter();
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
}