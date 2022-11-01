using System;
using System.Threading;
using System.Threading.Tasks;

public static class TaskExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="timeout"></param>
    /// <returns>Is TimeOut(返回是否超时)</returns>
    public static async Task<bool> WaitAsync(this Task task, TimeSpan timeout)
    {
        using(var timeoutCancellationTokenSource = new CancellationTokenSource()) 
        {
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
}