namespace Geek.Server.Core.Actors.Impl
{
    public abstract class WorkWrapper
    {
        public WorkerActor Owner { get; set; }
        public int TimeOut { get; set; }
        public abstract Task DoTask();
        public abstract string GetTrace();
        public abstract void ForceSetResult();
        public long CallChainId { get; set; }
        protected void SetContext()
        {
            RuntimeContext.SetContext(CallChainId, Owner.Id);
            Owner.CurChainId = CallChainId;
        }
        public void ResetContext()
        {
            Owner.CurChainId = 0;
        }
    }

    public class ActionWrapper : WorkWrapper
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Action Work { private set; get; }
        public TaskCompletionSource<bool> Tcs { private set; get; }

        public ActionWrapper(Action work)
        {
            Work = work;
            Tcs = new TaskCompletionSource<bool>();
        }

        public override Task DoTask()
        {
            try
            {
                SetContext();
                Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                ResetContext();
                Tcs.TrySetResult(true);
            }
            return Task.CompletedTask;
        }

        public override string GetTrace()
        {
            return Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            ResetContext();
            Tcs.TrySetResult(false);
        }
    }

    public class FuncWrapper<T> : WorkWrapper
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Func<T> Work { private set; get; }
        public TaskCompletionSource<T> Tcs { private set; get; }

        public FuncWrapper(Func<T> work)
        {
            Work = work;
            Tcs = new TaskCompletionSource<T>();
        }

        public override Task DoTask()
        {
            T ret = default;
            try
            {
                SetContext();
                ret = Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                ResetContext();
                Tcs.TrySetResult(ret);
            }
            return Task.CompletedTask;
        }

        public override string GetTrace()
        {
            return Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            ResetContext();
            Tcs.TrySetResult(default);
        }
    }

    public class ActionAsyncWrapper : WorkWrapper
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Func<Task> Work { private set; get; }
        public TaskCompletionSource<bool> Tcs { private set; get; }

        public ActionAsyncWrapper(Func<Task> work)
        {
            Work = work;
            Tcs = new TaskCompletionSource<bool>();
        }

        public override async Task DoTask()
        {
            try
            {
                SetContext();
                await Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                ResetContext();
                Tcs.TrySetResult(true);
            }
        }

        public override string GetTrace()
        {
            return Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            ResetContext();
            Tcs.TrySetResult(false);
        }
    }

    public class FuncAsyncWrapper<T> : WorkWrapper
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Func<Task<T>> Work { private set; get; }
        public TaskCompletionSource<T> Tcs { private set; get; }

        public FuncAsyncWrapper(Func<Task<T>> work)
        {
            Work = work;
            Tcs = new TaskCompletionSource<T>();
        }

        public override async Task DoTask()
        {
            T ret = default;
            try
            {
                SetContext();
                ret = await Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                ResetContext();
                Tcs.TrySetResult(ret);
            }
        }

        public override string GetTrace()
        {
            return Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            ResetContext();
            Tcs.TrySetResult(default);
        }
    }
}
