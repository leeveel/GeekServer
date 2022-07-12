using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Geek.Client
{
    public class UniActor
    {

        private ActionBlock<WorkWrapper> actionBlock = null;

        public UniActor()
        {
            actionBlock = new ActionBlock<WorkWrapper>(
                DoTask,
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 1,
                    //指定为Unity的Context
                    //https://docs.microsoft.com/zh-cn/dotnet/standard/parallel-programming/how-to-specify-a-task-scheduler-in-a-dataflow-block
                    TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()
                }); ;
        }

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
                UnityEngine.Debug.LogError("actor 执行超时 强制结束:" + wrapper.GetTrace());
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

