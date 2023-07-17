using System.Threading.Tasks.Dataflow;
using Geek.Server.Core.Utils;
using NLog;

namespace Geek.Server.Core.Actors.Impl
{
    public class WorkerActor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal long CurChainId { get; set; }
        internal long Id { get; init; }
        public const int TIME_OUT = 13000;

        public WorkerActor(long id = 0)
        {
            if (id == 0)
                id = IdGenerator.GetUniqueId(IDModule.WorkerActor);
            Id = id;
            ActionBlock = new ActionBlock<WorkWrapper>(InnerRun, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });
        }

        private static async Task InnerRun(WorkWrapper wrapper)
        {
            var task = wrapper.DoTask();
            try
            {
                await task.WaitAsync(TimeSpan.FromMilliseconds(wrapper.TimeOut));
            }
            catch (TimeoutException)
            {
                Log.Fatal("wrapper执行超时:" + wrapper.GetTrace());
                //强制设状态-取消该操作
                wrapper.ForceSetResult();
            }
        }

        private ActionBlock<WorkWrapper> ActionBlock { get; init; }

        /// <summary>
        /// chainId == 0说明是新的异步环境
        /// chainId相等说明是一直await下去的（一种特殊情况是自己入自己的队）
        /// </summary>
        /// <returns></returns>
        public (bool needEnqueue, long chainId) IsNeedEnqueue()
        {
            var chainId = RuntimeContext.CurChainId;
            bool needEnqueue = chainId == 0 || chainId != CurChainId;
            if (needEnqueue && chainId == 0) chainId = NextChainId();
            return (needEnqueue, chainId);
        }

        #region 勿调用(仅供代码生成器调用)
        public Task Enqueue(Action work, long callChainId, bool discard = false, int timeOut = TIME_OUT)
        {
            if (!discard && Settings.IsDebug && !ActorLimit.AllowCall(Id))
                return default;
            var at = new ActionWrapper(work)
            {
                Owner = this,
                TimeOut = timeOut,
                CallChainId = callChainId
            };
            ActionBlock.SendAsync(at);
            return at.Tcs.Task;
        }
        public Task<T> Enqueue<T>(Func<T> work, long callChainId, bool discard = false, int timeOut = TIME_OUT)
        {
            if (!discard && Settings.IsDebug && !ActorLimit.AllowCall(Id))
                return default;
            var at = new FuncWrapper<T>(work)
            {
                Owner = this,
                TimeOut = timeOut,
                CallChainId = callChainId
            };
            ActionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task Enqueue(Func<Task> work, long callChainId, bool discard = false, int timeOut = TIME_OUT)
        {
            if (!discard && Settings.IsDebug && !ActorLimit.AllowCall(Id))
                return default;
            var at = new ActionAsyncWrapper(work)
            {
                Owner = this,
                TimeOut = timeOut,
                CallChainId = callChainId
            };
            ActionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> Enqueue<T>(Func<Task<T>> work, long callChainId, bool discard = false, int timeOut = TIME_OUT)
        {
            if (!discard && Settings.IsDebug && !ActorLimit.AllowCall(Id))
                return default;
            var at = new FuncAsyncWrapper<T>(work)
            {
                Owner = this,
                TimeOut = timeOut,
                CallChainId = callChainId
            };
            ActionBlock.SendAsync(at);
            return at.Tcs.Task;
        }
        #endregion

        #region 供框架底层调用(逻辑开发人员应尽量避免调用)
        public void Tell(Action work, int timeout = Actor.TIME_OUT)
        {
            var at = new ActionWrapper(work)
            {
                Owner = this,
                TimeOut = timeout,
                CallChainId = NextChainId(),
            };
            _ = ActionBlock.SendAsync(at);
        }

        public void Tell(Func<Task> work, int timeout = Actor.TIME_OUT)
        {
            var wrapper = new ActionAsyncWrapper(work)
            {
                Owner = this,
                TimeOut = timeout,
                CallChainId = NextChainId(),
            };
            _ = ActionBlock.SendAsync(wrapper);
        }

        /// <summary>
        /// 调用该方法禁止丢弃Task，丢弃Task请使用Tell方法
        /// </summary>
        public Task SendAsync(Action work, int timeout = Actor.TIME_OUT)
        {
            (bool needEnqueue, long chainId) = IsNeedEnqueue();
            if (needEnqueue)
            {
                if (Settings.IsDebug && !ActorLimit.AllowCall(Id))
                    return default;

                var at = new ActionWrapper(work)
                {
                    Owner = this,
                    TimeOut = timeout,
                    CallChainId = chainId,
                };
                ActionBlock.SendAsync(at);
                return at.Tcs.Task;
            }
            else
            {
                work();
                return Task.CompletedTask;
            }
        }

        public Task<T> SendAsync<T>(Func<T> work, int timeout = Actor.TIME_OUT)
        {
            (bool needEnqueue, long chainId) = IsNeedEnqueue();
            if (needEnqueue)
            {
                if (Settings.IsDebug && !ActorLimit.AllowCall(Id))
                    return default;

                var at = new FuncWrapper<T>(work)
                {
                    Owner = this,
                    TimeOut = timeout,
                    CallChainId = chainId,
                };
                ActionBlock.SendAsync(at);
                return at.Tcs.Task;
            }
            else
            {
                return Task.FromResult(work());
            }
        }

        public Task SendAsync(Func<Task> work, int timeout = Actor.TIME_OUT, bool checkLock = true)
        {
            (bool needEnqueue, long chainId) = IsNeedEnqueue();
            if (needEnqueue)
            {
                if (checkLock && Settings.IsDebug && !ActorLimit.AllowCall(Id))
                    return default;

                var wrapper = new ActionAsyncWrapper(work)
                {
                    Owner = this,
                    TimeOut = timeout,
                    CallChainId = chainId,
                };
                ActionBlock.SendAsync(wrapper);
                return wrapper.Tcs.Task;
            }
            else
            {
                return work();
            }
        }

        public Task<T> SendAsync<T>(Func<Task<T>> work, int timeout = Actor.TIME_OUT)
        {
            (bool needEnqueue, long chainId) = IsNeedEnqueue();
            if (needEnqueue)
            {
                if (Settings.IsDebug && !ActorLimit.AllowCall(Id))
                    return default;

                var wrapper = new FuncAsyncWrapper<T>(work)
                {
                    Owner = this,
                    TimeOut = timeout,
                    CallChainId = chainId,
                };
                ActionBlock.SendAsync(wrapper);
                return wrapper.Tcs.Task;
            }
            else
            {
                return work();
            }
        }
        #endregion

        private static long chainId = DateTime.Now.Ticks;

        /// <summary>
        /// 调用链生成
        /// </summary>
        public static long NextChainId()
        {
            var id = Interlocked.Increment(ref chainId);
            if (id == 0)
            {
                id = Interlocked.Increment(ref chainId);
            }
            return id;
        }
    }
}
