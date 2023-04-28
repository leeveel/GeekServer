using Geek.Server.Core.Utils;

namespace Geek.Server.Core.Net
{
    public class ReConnecter
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private readonly Func<Task<bool>> connFun;
        private readonly int retryTimes;
        private int retryed = 0;
        /// <summary>
        /// 重试延迟(毫秒)
        /// </summary>
        private readonly int retryDelay = 15000;

        /// <summary>
        /// 两分钟
        /// </summary>
        const int MAX_DELAY = 120_000;

        private readonly string connInfo;

        public ReConnecter(Func<Task<bool>> connFun, string connInfo = "", int retryTimes = -1, int retryDelay = 5000)
        {
            this.connFun = connFun;
            this.connInfo = connInfo;
            this.retryDelay = retryDelay;
            this.retryTimes = retryTimes;
        }

        public async Task<bool> Connect()
        {
            if (connFun != null)
            {
                bool success = await connFun();
                if (!success)
                    await ReConnect();
                else
                    retryed = 0;
                return success;
            }
            else
            {
                return false;
            }
        }

        public void Close()
        {
            cancel?.Cancel();
            cancel = null;
        }

        /// <summary>
        /// 尝试立即重连
        /// </summary>
        /// <returns></returns>
        public async Task TryReConnectImmediately()
        {
            try
            {
                cancel?.Cancel();
                cancel = null;
                retryed = Math.Max(0, retryed - 1);
                await Connect();
            }
            catch (Exception)
            {

            }
        }

        private CancellationTokenSource cancel;
        public async Task ReConnect()
        {
            try
            {
                if (!Settings.AppRunning)
                    return;
                if (retryTimes < 0 || retryed < retryTimes)
                {
                    //每失败三次通知一次
                    if (retryed != 0 && retryed % 3 == 0)
                        NotifyDingTalk();
                    int delay = GetNextDelayTime();
                    retryed++;
                    LOGGER.Error($"连接断开,{delay}ms后尝试重连");
                    cancel = new CancellationTokenSource();
                    await Task.Delay(delay, cancel.Token);
                    if (!cancel.IsCancellationRequested)
                        await Connect();
                    cancel = null;
                }
                else
                {
                    NotifyDingTalk();
                }
            }
            catch (TaskCanceledException)
            {
                LOGGER.Error($"重连取消,尝试次数:{retryed}");
            }
        }

        public void NotifyDingTalk()
        {
            _ = ExceptionMonitor.Report(ExceptionType.NetworkTimeout, $"{connInfo},重连失败,尝试次数:{retryed}");
        }

        private int GetNextDelayTime()
        {
            return Math.Min(retryDelay + retryed * 5000, MAX_DELAY);
        }

    }
}
