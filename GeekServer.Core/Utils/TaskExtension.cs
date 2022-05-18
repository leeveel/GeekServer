using System;
using System.Threading;
using System.Threading.Tasks;

public static class TaskExtension
{
    public class WaitAsyncInfo<T>
    {
        public T Res;
        public bool IsTimeOut;
    }

    /// <summary>
    /// 异步等待+超时设定
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="task"></param>
    /// <param name="timeout"></param>
    /// <returns>不需要明确知道是否超时</returns>
    public static async Task<TResult> WaitAsyncCustom<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        using var timeoutCancellationTokenSource = new CancellationTokenSource();
        var delayTask = Task.Delay(timeout, timeoutCancellationTokenSource.Token);
        if (await Task.WhenAny(task, delayTask) == task)
        {
            timeoutCancellationTokenSource.Cancel();
            return await task;  // Very important in order to propagate exceptions
        }
        else
        {
            //throw new TimeoutException("The operation has timed out.");
            return default;
        }
    }

    /// <summary>
    /// 异步等待+超时设定
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="task"></param>
    /// <param name="timeout"></param>
    /// <returns>需要明确知道是否超时</returns>
    public static async Task<WaitAsyncInfo<TResult>> WaitDetailAsync<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        using var timeoutCancellationTokenSource = new CancellationTokenSource();
        var delayTask = Task.Delay(timeout, timeoutCancellationTokenSource.Token);
        if (await Task.WhenAny(task, delayTask) == task)
        {
            timeoutCancellationTokenSource.Cancel();
            var r = await task;  // Very important in order to propagate exceptions
            var res = new WaitAsyncInfo<TResult>
            {
                Res = r,
                IsTimeOut = false
            };
            return res;
        }
        else
        {
            var res = new WaitAsyncInfo<TResult>();
            res.IsTimeOut = true;
            return res;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="timeout"></param>
    /// <returns>返回是否超时</returns>
    public static async Task<bool> WaitAsyncCustom(this Task task, TimeSpan timeout)
    {
        using var timeoutCancellationTokenSource = new CancellationTokenSource();
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
        if (completedTask == task)
        {
            timeoutCancellationTokenSource.Cancel();
            await task;  // Very important in order to propagate exceptions
            return false;
        }
        else
        {
            //Console.WriteLine("WaitAsync timed out");
            //throw new TimeoutException("The operation has timed out.");
            return true;
        }
    }
}