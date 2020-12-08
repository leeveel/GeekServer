/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using System.Threading.Tasks;

namespace Geek.Core.Actor
{
    public abstract class WorkWrapper
    {
        public ActorNode Node;

        public BaseActor owner;
        public abstract Task DoTask();
        public abstract string GetTrace();

        public abstract void ForceSetResult();

        public int TimeOut;
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
            if (Settings.Ins.isDebug)
                Node.actor.SetActiveNode(Node);

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
            if (Settings.Ins.isDebug)
                Node.actor.SetActiveNode(null);

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
            if (Settings.Ins.isDebug)
                Node.actor.SetActiveNode(Node);

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

            if (Settings.Ins.isDebug)
                Node.actor.SetActiveNode(null);

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
            if (Settings.Ins.isDebug)
                Node.actor.SetActiveNode(Node);

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
            if (Settings.Ins.isDebug)
                Node.actor.SetActiveNode(null);
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
            if (Settings.Ins.isDebug)
                Node.actor.SetActiveNode(Node);

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

            if (Settings.Ins.isDebug)
                Node.actor.SetActiveNode(null);
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
