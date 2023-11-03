using System.Collections;
using System.Runtime.InteropServices;

namespace Geek.Server.Core.Utils
{
    public class AppExitHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        static Action callBack;
        private static PosixSignalRegistration exitSignalReg;
        public static void Init(Action existCallBack)
        {
            callBack = () =>
            {
                Action act = null;
                lock (callBack)
                {
                    if (existCallBack != null)
                    {
                        act = existCallBack;
                    }
                    existCallBack = null;
                }
                act?.Invoke();
            };

            //退出监听 
            exitSignalReg = PosixSignalRegistration.Create(PosixSignal.SIGTERM, c =>
            {
                LOGGER.Info("PosixSignalRegistration SIGTERM....");
                callBack();
            });

            AppDomain.CurrentDomain.ProcessExit += (s, e) => { callBack(); };
            //Fetal异常监听
            AppDomain.CurrentDomain.UnhandledException += (s, e) => { handleFetalException("AppDomain.CurrentDomain.UnhandledException", e.ExceptionObject); };

            TaskScheduler.UnobservedTaskException += (s, e) => { handleFetalException("TaskScheduler.UnobservedTaskException", e.Exception); };

            //ctrl+c
            Console.CancelKeyPress += (s, e) => { callBack(); };
        }

        static void handleFetalException(string tag, object e)
        {
            //这里可以发送短信或者钉钉消息通知到运维
            LOGGER.Error($"[{tag}]get unhandled exception");
            if (e == null)
            {
                callBack();
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
            //callBack();
        }
    }
}
