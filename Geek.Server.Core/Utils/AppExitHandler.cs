using System.Collections;
using System.Runtime.InteropServices;

namespace Geek.Server.Core.Utils
{
    public static class AppExitHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        static Action callBack;
        private static PosixSignalRegistration exitSignalReg;
        static bool isKill = false;
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

            exitSignalReg = PosixSignalRegistration.Create(PosixSignal.SIGTERM, c =>
            {
                LOGGER.Info("PosixSignalRegistration SIGTERM....");
                callBack();
            });

            //退出监听 
            AppDomain.CurrentDomain.ProcessExit += (s, e) => { callBack(); };
            //Fetal异常监听
            AppDomain.CurrentDomain.UnhandledException += (s, e) => { handleFetalException("AppDomain.CurrentDomain.UnhandledException", e.ExceptionObject); };

            TaskScheduler.UnobservedTaskException += (s, e) => { handleFetalException("TaskScheduler.UnobservedTaskException", e.Exception); };

            //ctrl+c
            Console.CancelKeyPress += (s, e) => { callBack(); };
        }

        public static void Kill()
        {
            isKill = true;
        }

        static void handleFetalException(string tag, object e)
        {
            if (isKill)
                return;
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
            //callBack();
        }
    }
}
