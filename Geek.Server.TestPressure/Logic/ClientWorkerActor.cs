using System.Threading.Tasks.Dataflow;
using Geek.Server.Core.Actors.Impl;

namespace Geek.Server.TestPressure.Logic
{
    public class ClientWorkerActor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ActionBlock<WorkWrapper> actionBlock = null;

        public ClientWorkerActor()
        {
            actionBlock = new ActionBlock<WorkWrapper>(
                DoTask,
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 1,
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
                Log.Error("actor 执行超时 强制结束:" + wrapper.GetTrace());
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
    }
}

