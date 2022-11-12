using System;
using System.Threading.Tasks;

namespace Geek.Client
{
    /// <summary>
    /// 强烈推荐使用UniTask
    /// </summary>
    public class UniActor
    {

        private readonly TaskFactory taskFactory;
        public UniActor()
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            taskFactory = new TaskFactory(scheduler); 
        }

        const int TIME_OUT = -1;
        static async Task DoTask(object arg)
        {
            var wrapper = (WorkWrapper)arg;
            if (wrapper == null)
            {
                UnityEngine.Debug.LogError("UniActor.DoTask参数不是WorkWrapper类型");
                return;
            }
            if (wrapper.TimeOut == -1)
            {
                await wrapper.DoTask();
            }
            else
            {
                var task = wrapper.DoTask();
                var res = await task.WaitAsync(TimeSpan.FromMilliseconds(wrapper.TimeOut));
                if (res)
                {
                    UnityEngine.Debug.LogError("wrapper执行超时:" + wrapper.GetTrace());
                    //强制设状态-取消该操作
                    wrapper.ForceSetResult();
                }
            }
        }

        public Task SendAsync(Action work, int timeOut = TIME_OUT)
        {
            var wrapper = new ActionWrapper(work);
            wrapper.TimeOut = timeOut;
            taskFactory.StartNew(DoTask, wrapper);
            return wrapper.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<T> work, int timeOut = TIME_OUT)
        {
            var wrapper = new FuncWrapper<T>(work);
            wrapper.TimeOut = timeOut;
            taskFactory.StartNew(DoTask, wrapper);
            return wrapper.Tcs.Task;
        }

        public Task SendAsync(Func<Task> work, int timeOut = TIME_OUT)
        {
            var wrapper = new ActionAsyncWrapper(work);
            wrapper.TimeOut = timeOut;
            taskFactory.StartNew(DoTask, wrapper);
            return wrapper.Tcs.Task;
        }

        public Task<T> SendAsync<T>(Func<Task<T>> work, int timeOut = TIME_OUT)
        {
            var wrapper = new FuncAsyncWrapper<T>(work);
            wrapper.TimeOut = timeOut;
            taskFactory.StartNew(DoTask, wrapper);
            return wrapper.Tcs.Task;
        }

    }
}

