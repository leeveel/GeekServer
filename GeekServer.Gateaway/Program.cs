
using Geek.Server.Proto;
using Geek.Server.Gateaway.MessageHandler;
using System.Collections;
using System.Diagnostics;
using TcpServer = Geek.Server.Gateaway.Net.Tcp.TcpServer;

namespace Geek.Server.Gateaway
{
    class Program
    {
        static volatile bool ExitCalled = false;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static volatile Task ShutDownTask = null;
        private static volatile Task MainLoopTask = null;

        public static async Task Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("Configs/gate_NLog.config");

            Settings.Load<GateSettings>("Configs/gate_config.json", ServerType.Gate);

            PolymorphicRegister.Load();
            MsgHanderFactory.Init();
            HttpHanderFactory.Init();
            AddExitHandler();

            MainLoopTask = Start();
            await MainLoopTask;

            if (ShutDownTask != null)
                await ShutDownTask;
        }

        static async Task Start()
        {
            await RpcServer.Start(Settings.InsAs<GateSettings>().RpcPort);
            await TcpServer.Start(Settings.TcpPort);
            await HttpServer.Start(Settings.HttpPort);
            Settings.AppRunning = true;
            TimeSpan delay = TimeSpan.FromSeconds(1);
            while (Settings.AppRunning)
            {
                await Task.Delay(delay);
            }
            await RpcServer.Stop();
            await TcpServer.Stop();
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
                Settings.AppRunning = false;
                MainLoopTask?.Wait();
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