using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Geek.Client
{
    public class Actor
    {
        ActionBlock<WorkWrapper> actionBlock = new ActionBlock<WorkWrapper>(DoTask);
        const int TIME_OUT = 10000;
        static async Task DoTask(WorkWrapper wrapper)
        {
            var task = wrapper.DoTask();
            var tokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(wrapper.TimeOut, tokenSource.Token));
            if (completedTask == task)
            {
                tokenSource.Cancel();
                await task;
            }
            else
            {
                Debuger.Err("actor 执行超时 强制结束:" + wrapper.GetTrace());
                wrapper.ForceSetResult();
            }
        }

        public Task SendAsync(Action work, int timeOut = TIME_OUT)
        {
            var wrapper = new ActionWrapper(work);
            wrapper.TimeOut = timeOut;
            actionBlock.SendAsync(wrapper);
            return wrapper.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<T> work, int timeOut = TIME_OUT)
        {
            var wrapper = new FuncWrapper<T>(work);
            wrapper.TimeOut = timeOut;
            actionBlock.SendAsync(wrapper);
            return wrapper.Tcs.Task;
        }

        public Task SendAsync(Func<Task> work, int timeOut = TIME_OUT)
        {
            var wrapper = new ActionAsyncWrapper(work);
            wrapper.TimeOut = timeOut;
            actionBlock.SendAsync(wrapper);
            return wrapper.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<Task<T>> work, int timeOut = TIME_OUT)
        {
            var wrapper = new FuncAsyncWrapper<T>(work);
            wrapper.TimeOut = timeOut;
            actionBlock.SendAsync(wrapper);
            return wrapper.Tcs.Task;
        }
    }
}

