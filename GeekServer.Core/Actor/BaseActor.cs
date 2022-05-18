using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Geek.Server
{
    public abstract class BaseActor
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public const int TIME_OUT = 13000;

        /// <summary>
        /// 当前调用链id
        /// </summary>
        internal long curCallChainId;   
        private static long idCounter = 1;
        /// <summary>
        /// 当前任务是否可以被交错执行
        /// </summary>
        public volatile bool CurCanBeInterleaved;

        internal int entityType;
        internal Type compType;
        public BaseActor(int parallelism = 1)
        {
            actionBlock = new ActionBlock<WorkWrapper>(InnerRun,  new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = parallelism });
        }

        readonly ActionBlock<WorkWrapper> actionBlock;

        static async Task InnerRun(WorkWrapper wrapper)
        {
            if (wrapper.TimeOut == -1)
            {
                await wrapper.DoTask();
            }
            var task = wrapper.DoTask();
            var res = await task.WaitAsyncCustom(TimeSpan.FromMilliseconds(wrapper.TimeOut));
            if (res)
            {
                LOGGER.Fatal("wrapper执行超时:" + wrapper.GetTrace());
                //强制设状态-取消该操作
                wrapper.ForceSetResult();
            }
        }

        public long IsNeedEnqueue()
        {
            long callChainId = RuntimeContext.Current;
            if (callChainId > 0)
            {
                if (callChainId == curCallChainId)
                    return -1;
                return callChainId;
            }
            return NewChainId();
        }

        public long NewChainId()
        {
            return Interlocked.Increment(ref idCounter);
        }

        public Task Enqueue(Action work, long callChainId, int timeOut = TIME_OUT)
        {
            ActionWrapper at = new ActionWrapper(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            at.CallChainId = callChainId;
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task SendAsync(Action work, int timeOut = TIME_OUT)
        {
            ActionWrapper at = new ActionWrapper(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            at.CallChainId = Interlocked.Increment(ref idCounter);
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> Enqueue<T>(Func<T> work, long callChainId, int timeOut = TIME_OUT)
        {
            FuncWrapper<T> at = new FuncWrapper<T>(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            at.CallChainId = callChainId;
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<T> work, int timeOut = TIME_OUT)
        {
            FuncWrapper<T> at = new FuncWrapper<T>(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            at.CallChainId = Interlocked.Increment(ref idCounter);
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task Enqueue(Func<Task> work, long callChainId, int timeOut = TIME_OUT)
        {
            ActionAsyncWrapper at = new ActionAsyncWrapper(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            at.CallChainId = callChainId;
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task SendAsync(Func<Task> work, int timeOut = TIME_OUT)
        {
            ActionAsyncWrapper at = new ActionAsyncWrapper(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            at.CallChainId = Interlocked.Increment(ref idCounter);
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> Enqueue<T>(Func<Task<T>> work, long callChainId, int timeOut = TIME_OUT)
        {
            FuncAsyncWrapper<T> at = new FuncAsyncWrapper<T>(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            at.CallChainId = callChainId;
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<Task<T>> work, int timeOut = TIME_OUT)
        {
            FuncAsyncWrapper<T> at = new FuncAsyncWrapper<T>(work);
            at.Owner = this;
            at.TimeOut = timeOut;
            at.CallChainId = Interlocked.Increment(ref idCounter);
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public virtual Task<bool> ReadyToDeactive()
        {
            return Task.FromResult(true);
        }
    }
}
