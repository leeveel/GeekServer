using System;
using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class WorkWrapper
    {
        public ActorNode Node { get; set; }
        public BaseActor Owner { get; set; }
        public int TimeOut { get; set; }
        public abstract Task DoTask();
        public abstract string GetTrace();
        public abstract void ForceSetResult();
    }

    public class ActionWrapper : WorkWrapper
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Action Work { private set; get; }
        public TaskCompletionSource<bool> Tcs { private set; get; }

        public ActionWrapper(Action work)
        {
            this.Work = work;
            Tcs = new TaskCompletionSource<bool>();
        }

        public override Task DoTask()
        {
            if (Settings.Ins.IsDebug)
                Node.Actor.SetActiveNode(Node);

            try
            {
                Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                Tcs.TrySetResult(true);
            }
            if (Settings.Ins.IsDebug)
                Node.Actor.SetActiveNode(null);

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
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Func<T> Work { private set; get; }
        public TaskCompletionSource<T> Tcs { private set; get; }

        public FuncWrapper(Func<T> work)
        {
            this.Work = work;
            this.Tcs = new TaskCompletionSource<T>();
        }

        public override Task DoTask()
        {
            if (Settings.Ins.IsDebug)
                Node.Actor.SetActiveNode(Node);

            T ret = default;
            try
            {
                ret = Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                Tcs.TrySetResult(ret);
            }

            if (Settings.Ins.IsDebug)
                Node.Actor.SetActiveNode(null);

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
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Func<Task> Work { private set; get; }
        public TaskCompletionSource<bool> Tcs { private set; get; }

        public ActionAsyncWrapper(Func<Task> work)
        {
            this.Work = work;
            Tcs = new TaskCompletionSource<bool>();
        }

        public override async Task DoTask()
        {
            if (Settings.Ins.IsDebug)
                Node.Actor.SetActiveNode(Node);

            try
            {
                await Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                Tcs.TrySetResult(true);
            }
            if (Settings.Ins.IsDebug)
                Node.Actor.SetActiveNode(null);
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
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Func<Task<T>> Work { private set; get; }
        public TaskCompletionSource<T> Tcs { private set; get; }

        public FuncAsyncWrapper(Func<Task<T>> work)
        {
            this.Work = work;
            this.Tcs = new TaskCompletionSource<T>();
        }

        public override async Task DoTask()
        {
            if (Settings.Ins.IsDebug)
                Node.Actor.SetActiveNode(Node);

            T ret = default;
            try
            {
                ret = await Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                Tcs.TrySetResult(ret);
            }

            if (Settings.Ins.IsDebug)
                Node.Actor.SetActiveNode(null);
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
