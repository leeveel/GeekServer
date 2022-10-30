
using Geek.Server.Proto;
using GeekServer.Gateaway.MessageHandler;
using GeekServer.Gateaway.Net.Rpc;
using GeekServer.Gateaway.Net.Tcp;
using NLog;
using NLog.Fluent;
using System.Collections;
using System.Diagnostics;

namespace GeekServer.Gateaway
{
    class Program
    {
        static volatile bool ExitCalled = false;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static volatile Task ShutDownTask = null;
        public static async Task Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("Configs/gate_NLog.config");
            Settings.Load("Configs/gate_config.json");

            PolymorphicRegister.Load();
            MsgHanderFactory.Init();
            AddExitHandler();

            await RpcServer.Start(Settings.Ins.RpcPort);
            await TcpServer.Start(Settings.Ins.TcpPort);

            if (ShutDownTask != null)
                await ShutDownTask;
        }


        public static void AddExitHandler()
        {
            //退出监听
            AppDomain.CurrentDomain.ProcessExit += (s, e) => { HandleExit(); };
            //Fetal异常监听
            AppDomain.CurrentDomain.UnhandledException += (s, e) => { HandleFetalException(e.ExceptionObject); };
            //ctrl+c
            Console.CancelKeyPress += (s, e) => { HandleExit(); };
        }

        private static void HandleExit()
        {
            if (ExitCalled)
                return;
            ExitCalled = true;
            Log.Info($"监听到退出程序消息");
            ShutDownTask = Task.Run(() =>
            {
                Settings.Ins.AppRunning = false;
                LogManager.Shutdown();
                Console.WriteLine($"退出程序");
                Process.GetCurrentProcess().Kill();
            });
            ShutDownTask.Wait();
        }

        private static void HandleFetalException(object e)
        {
            //这里可以发送短信或者钉钉消息通知到运维
            Log.Error("get unhandled exception");
            if (e is IEnumerable arr)
            {
                foreach (var ex in arr)
                    Log.Error($"Unhandled Exception:{ex}");
            }
            else
            {
                Log.Error($"Unhandled Exception:{e}");
            }
        }
    }
}