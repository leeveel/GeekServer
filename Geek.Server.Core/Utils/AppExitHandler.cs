using System.Collections;

namespace Geek.Server.Core.Utils
{
    public static class AppExitHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        static Action callBack;
        public static void Init(Action exitCallBack)
        {
            callBack = exitCallBack;
            OnExit(exitCallBack);
            //Fetal异常监听
            AppDomain.CurrentDomain.UnhandledException += (s, e) => { HandleFetalException(e.ExceptionObject); };
        }

        public static void OnExit(Action exitCallBack)
        {
            //退出监听
            AppDomain.CurrentDomain.ProcessExit += (s, e) => { exitCallBack?.Invoke(); };
            //ctrl+c
            Console.CancelKeyPress += (s, e) => { exitCallBack?.Invoke(); };
        }

        private static void HandleFetalException(object e)
        {
            //发送钉钉消息通知到运维
            ExceptionMonitor.Report(ExceptionType.UnhandledException, $"服务器异常退出 e:{e}").Wait(TimeSpan.FromSeconds(10));

            LOGGER.Error("get unhandled exception");
            if (e is IEnumerable arr)
            {
                foreach (var ex in arr)
                    LOGGER.Error($"Unhandled Exception:{ex}");
            }
            else
            {
                LOGGER.Error($"Unhandled Exception:{e}");
            }
            callBack?.Invoke();
        }
    }
}
