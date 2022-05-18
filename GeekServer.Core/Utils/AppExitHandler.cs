using System;
using System.Collections;

namespace Geek.Server
{
    public class AppExitHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        static Action callBack;
        public static void Init(Action existCallBack)
        {
            callBack = existCallBack;
            //退出监听
            AppDomain.CurrentDomain.ProcessExit += (s, e) => { callBack?.Invoke(); };
            //Fetal异常监听
            AppDomain.CurrentDomain.UnhandledException += (s, e) => { handleFetalException(e.ExceptionObject); };
            //ctrl+c
            Console.CancelKeyPress += (s, e) => { callBack?.Invoke(); };
        }

        static void handleFetalException(object e)
        {
            var args = (UnhandledExceptionEventArgs)e;
            //这里可以发送短信或者钉钉消息通知到运维
            LOGGER.Error("get unhandled exception");
            if (e == null)
            {
                callBack?.Invoke();
                return;
            }
            if (e is Array arr)
            {
                foreach (var ex in arr)
                    LOGGER.Error("Unhandled Exception:" + ex.ToString());
            }
            else if (e is IList list)
            {
                foreach (var ex in list)
                    LOGGER.Error("Unhandled Exception:" + ex.ToString());
            }
            else
            {
                LOGGER.Error("Unhandled Exception:" + e.ToString());
            }
            ExceptionMonitor.Report(ExceptionType.UnhandledException, $"{e}").Wait(TimeSpan.FromSeconds(10));
            callBack?.Invoke();
        }
    }
}
