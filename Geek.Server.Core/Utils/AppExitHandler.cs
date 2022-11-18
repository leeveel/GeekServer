using System.Collections;

namespace Geek.Server.Core.Utils
{
    public static class AppExitHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        static Action callBack;
        public static void Init(Action existCallBack)
        {
            callBack = existCallBack;
            //退出监听
            AppDomain.CurrentDomain.ProcessExit += (s, e) => { callBack?.Invoke(); };
            //Fetal异常监听
            AppDomain.CurrentDomain.UnhandledException += (s, e) => { HandleFetalException(e.ExceptionObject); };
            //ctrl+c
            Console.CancelKeyPress += (s, e) => { callBack?.Invoke(); };
        }

        private static void HandleFetalException(object e)
        {
            //这里可以发送短信或者钉钉消息通知到运维
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
