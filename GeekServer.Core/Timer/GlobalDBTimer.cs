

using System;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class GlobalDBTimer
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static GlobalDBTimer Singleton = new GlobalDBTimer();

        Task timerTask;
        internal volatile bool Working;
        public void Start()
        {
            if (Working)
                return;
            Working = true;
            LOGGER.Info("start global db timer...");
            timerTask = Task.Run(timerLoop);
        }

        async Task timerLoop()
        {
            var random = new System.Random();
            int onceDelta = 1000;
            int delayTime = 0;
            int saveTime = random.Next(Settings.Ins.DataFlushTimeMin, Settings.Ins.DataFlushTimeMax);
            while (delayTime < saveTime * 1000)
            {
                //不能一次性delay，退出程序时监听不到
                await Task.Delay(onceDelta);
                delayTime += onceDelta;
                if (!Working)
                    break;
            }
            
            while(Working)
            {
                var start = DateTime.Now;

                await StateComponent.TimerSave();
                var delta = DateTime.Now - start;
                LOGGER.Info("db timer save state time:{}毫秒", delta.TotalMilliseconds);
                if (!Working)
                    break;

                await EntityMgr.CheckIdle();
                delta = DateTime.Now - start;
                LOGGER.Info("db timer loop time:{}毫秒", delta.TotalMilliseconds);

                if (!Working)
                    break;

                int delay = 10000;
                if (delta.TotalSeconds < saveTime)
                    delay = (saveTime - (int)delta.TotalSeconds) * 1000;

                delayTime = 0;
                while (delayTime < delay)
                {
                    await Task.Delay(onceDelta);
                    delayTime += onceDelta;
                    if (!Working)
                        break;
                }
            }
            LOGGER.Info("exit db timer loop...");
        }

        public Task SaveAllStates()
        {
            return StateComponent.SaveAllState();
        }

        public async Task OnShutdown()
        {
            Working = false;
            if (timerTask != null)
                await timerTask;
#if DEBUG_MODE
            Geek.Server.BaseDBState.CheckCallChainEnable = false;
#endif
            await StateComponent.SaveAllState();
        }
    }
}
