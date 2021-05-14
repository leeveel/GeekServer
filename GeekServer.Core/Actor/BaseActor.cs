using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Geek.Server
{
    public abstract class BaseActor
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public const int TIME_OUT = 10000;

        public long ActorId { get; set; }
        public BaseActor(int parallelism = 1)
        {
            if (Settings.Ins.IsDebug)
                checkActor = new DeadlockCheckActor(this);
            actionBlock = new ActionBlock<WorkWrapper>(InnerRun, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = parallelism });
        }

        readonly ActionBlock<WorkWrapper> actionBlock;

        static async Task InnerRun(WorkWrapper wrapper)
        {
            var task = wrapper.DoTask();
            var res = await task.WaitAsync(TimeSpan.FromMilliseconds(wrapper.TimeOut));
            if (res)
            {
                LOGGER.Fatal("wrapper执行超时:" + wrapper.GetTrace());
                //强制设状态-取消该操作
                wrapper.ForceSetResult();
            }
        }

        DeadlockCheckActor checkActor;
        public void SetActiveNode(ActorNode node)
        {
            checkActor.SetActiveNode(node);
        }

        public Task SendAsync(Action work, bool checkLock = true, int timeOut = TIME_OUT)
        {
            if (Settings.Ins.IsDebug)
                return checkActor.SendAsync(work, checkLock, timeOut);

            ActionWrapper at = new ActionWrapper(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<T> work, bool checkLock = true, int timeOut = TIME_OUT)
        {
            if (Settings.Ins.IsDebug)
                return checkActor.SendAsync(work, checkLock, timeOut);

            FuncWrapper<T> at = new FuncWrapper<T>(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task SendAsync(Func<Task> work, bool checkLock = true, int timeOut = TIME_OUT)
        {
            if (Settings.Ins.IsDebug)
                return checkActor.SendAsync(work, checkLock, timeOut);

            ActionAsyncWrapper at = new ActionAsyncWrapper(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<Task<T>> work, bool checkLock = true, int timeOut = TIME_OUT)
        {
            if (Settings.Ins.IsDebug)
                return checkActor.SendAsync(work, checkLock, timeOut);

            FuncAsyncWrapper<T> at = new FuncAsyncWrapper<T>(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public abstract Task Active();

        public abstract Task Deactive();

        public virtual Task<bool> ReadyToDeactive()
        {
            return Task.FromResult(true);
        }
    }
}
