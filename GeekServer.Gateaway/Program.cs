
using NLog;
using NLog.Fluent;
using System.Collections;
using System.Diagnostics;

namespace GeekServer.Gateaway
{
    //处理各种协议的接入，同时支持TCP和UDP(KCP协议)，进行双栈通信。
    //连接管理，会话建立，数据包加解密(DH+RC4)。
    //透传解密后的原始数据流到后端（通过gRPC streaming)。
    //复用多路用户连接，到一条通往game的物理连接。
    //不断开连接切换后端业务。
    //唯一入口，安全隔离核心服务。
    class Program
    {
        static volatile bool ExitCalled = false;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static volatile Task ShutDownTask = null;
        public static async Task Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("Configs/gate_NLog.config");
            Settings.Load("Configs/gate_config.json");

            AddExitHandler();

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