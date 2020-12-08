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
using Geek.Core.Utils;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Geek.Core.Actor
{
    public abstract class BaseActor
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public const int TIME_OUT = 5000;

        public long ActorId { get; set; }
        public BaseActor(int parallelism = 1)
        {
            if (Settings.Ins.isDebug)
                checkActor = new DeadlockCheckActor(this);

            actionBlock = new ActionBlock<WorkWrapper>(InnerRun, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = parallelism });
        }

        private readonly ActionBlock<WorkWrapper> actionBlock;

        private static async Task InnerRun(WorkWrapper wrapper)
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

        public Task SendAsync(Action work, int timeOut = TIME_OUT)
        {
            if (Settings.Ins.isDebug)
                return checkActor.Enqueue(work);

            ActionWrapper at = new ActionWrapper(work);
            at.owner = this;
            at.TimeOut = timeOut;
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<T> work, int timeOut = TIME_OUT)
        {
            if (Settings.Ins.isDebug)
                return checkActor.Enqueue(work);

            FuncWrapper<T> at = new FuncWrapper<T>(work);
            at.owner = this;
            at.TimeOut = timeOut;
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task SendAsync(Func<Task> work, int timeOut = TIME_OUT)
        {
            if(Settings.Ins.isDebug)
                return checkActor.Enqueue(work);

            ActionAsyncWrapper at = new ActionAsyncWrapper(work);
            at.owner = this;
            at.TimeOut = timeOut;
            actionBlock.SendAsync(at);
            return at.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<Task<T>> work, int timeOut = TIME_OUT)
        {
            if (Settings.Ins.isDebug)
                return checkActor.Enqueue(work);

            FuncAsyncWrapper<T> at = new FuncAsyncWrapper<T>(work);
            at.owner = this;
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
