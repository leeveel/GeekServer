using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Geek.Server
{
    public abstract class BaseActor
    {
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public const int TIME_OUT = 10000;

        /// <summary>
        /// 当前调用链id
        /// </summary>
        internal long curCallChainId;   
        private static long idCounter = 1;
        /// <summary>
        /// 当前任务是否可以被交错执行
        /// </summary>
        public volatile bool CurCanBeInterleaved;
        public long ActorId { get; set; }
        public BaseActor(int parallelism = 1)
        {
            actionBlock = new ActionBlock<WorkWrapper>(InnerRun, 
                new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = parallelism });
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

        private void IsNeedEnqueue(out bool needEnqueue, out long callChainId)
        {
            callChainId = RuntimeContext.Current;
            if (callChainId <= 0)
            {
                callChainId = Interlocked.Increment(ref idCounter);
                needEnqueue = true;
                return;
            }
            else if (callChainId == curCallChainId)
            {
                needEnqueue = false;
                return;
            }
            needEnqueue = true;
        }

        public Task SendAsync(Action work, bool isAwait = true, int timeOut = TIME_OUT)
        {
            long callChainId;
            bool needEnqueue;
            if (!isAwait)
            {
                callChainId = Interlocked.Increment(ref idCounter);
                needEnqueue = true;
            }
            else
            {
                IsNeedEnqueue(out needEnqueue, out callChainId);
            }
            if (needEnqueue)
            {
                ActionWrapper at = new ActionWrapper(work);
                at.Owner = this;
                at.TimeOut = timeOut;
                at.CallChainId = callChainId;
                actionBlock.SendAsync(at);
                return at.Tcs.Task;
            }
            else
            {
                work();
                return Task.CompletedTask;
            }
        }

        public Task<T> SendAsync<T>(Func<T> work, bool isAwait = true, int timeOut = TIME_OUT)
        {
            long callChainId;
            bool needEnqueue;
            if (!isAwait)
            {
                callChainId = Interlocked.Increment(ref idCounter);
                needEnqueue = true;
            }
            else
            {
                IsNeedEnqueue(out needEnqueue, out callChainId);
            }
            if (needEnqueue)
            {
                FuncWrapper<T> at = new FuncWrapper<T>(work);
                at.Owner = this;
                at.TimeOut = timeOut;
                at.CallChainId = callChainId;
                actionBlock.SendAsync(at);
                return at.Tcs.Task;
            }
            else
            {
                return Task.FromResult(work());
            }
        }

        public Task SendAsync(Func<Task> work, bool isAwait = true, int timeOut = TIME_OUT)
        {
            long callChainId;
            bool needEnqueue;
            if (!isAwait)
            {
                callChainId = Interlocked.Increment(ref idCounter);
                needEnqueue = true;
            }
            else
            {
                IsNeedEnqueue(out needEnqueue, out callChainId);
            }
            if (needEnqueue)
            {
                ActionAsyncWrapper at = new ActionAsyncWrapper(work);
                at.Owner = this;
                at.TimeOut = timeOut;
                at.CallChainId = callChainId;
                actionBlock.SendAsync(at);
                return at.Tcs.Task;
            }
            else
            {
                return work();
            }
        }

        public Task<T> SendAsync<T>(Func<Task<T>> work, bool isAwait = true, int timeOut = TIME_OUT)
        {
            long callChainId;
            bool needEnqueue;
            if (!isAwait)
            {
                callChainId = Interlocked.Increment(ref idCounter);
                needEnqueue = true;
            }
            else
            {
                IsNeedEnqueue(out needEnqueue, out callChainId);
            }
            if (needEnqueue)
            {
                FuncAsyncWrapper<T> at = new FuncAsyncWrapper<T>(work);
                at.Owner = this;
                at.TimeOut = timeOut;
                at.CallChainId = callChainId;
                actionBlock.SendAsync(at);
                return at.Tcs.Task;
            }
            else
            {
                return work();
            }
        }

        public virtual Task<bool> ReadyToDeactive()
        {
            return Task.FromResult(true);
        }
    }
}
