using System;
using System.Threading.Tasks;

namespace Geek.Client
{
    public abstract class WorkWrapper
    {
        public int TimeOut = -1;
        public abstract Task DoTask();
        public abstract string GetTrace();
        public abstract void ForceSetResult();
    }

    public class ActionWrapper : WorkWrapper
    {
        public Action Work { private set; get; }

        public TaskCompletionSource<bool> Tcs { private set; get; }

        public ActionWrapper(Action work)
        {
            this.Work = work;
            Tcs = new TaskCompletionSource<bool>();
        }

        public override Task DoTask()
        {
            try
            {
                Work();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                Tcs.TrySetResult(true);
            }

            return Task.CompletedTask;
        }

        public override string GetTrace()
        {
            return this.Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            Tcs.TrySetResult(false);
        }
    }

    public class FuncWrapper<T> : WorkWrapper
    {
        public Func<T> Work { private set; get; }

        public TaskCompletionSource<T> Tcs { private set; get; }

        public FuncWrapper(Func<T> work)
        {
            this.Work = work;
            this.Tcs = new TaskCompletionSource<T>();
        }

        public override Task DoTask()
        {
            T ret = default;
            try
            {
                ret = Work();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                Tcs.TrySetResult(ret);
            }

            return Task.CompletedTask;
        }

        public override string GetTrace()
        {
            return this.Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            Tcs.TrySetResult(default);
        }
    }

    public class ActionAsyncWrapper : WorkWrapper
    {
        public Func<Task> Work { private set; get; }

        public TaskCompletionSource<bool> Tcs { private set; get; }

        public ActionAsyncWrapper(Func<Task> work)
        {
            this.Work = work;
            Tcs = new TaskCompletionSource<bool>();
        }

        public override async Task DoTask()
        {
            try
            {
                await Work();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                Tcs.TrySetResult(true);
            }
        }

        public override string GetTrace()
        {
            return this.Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            Tcs.TrySetResult(false);
        }
    }

    public class FuncAsyncWrapper<T> : WorkWrapper
    {
        public Func<Task<T>> Work { private set; get; }

        public TaskCompletionSource<T> Tcs { private set; get; }

        public FuncAsyncWrapper(Func<Task<T>> work)
        {
            this.Work = work;
            this.Tcs = new TaskCompletionSource<T>();
        }

        public override async Task DoTask()
        {
            T ret = default;
            try
            {
                ret = await Work();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                Tcs.TrySetResult(ret);
            }
        }

        public override string GetTrace()
        {
            return this.Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            Tcs.TrySetResult(default);
        }
    }
}
