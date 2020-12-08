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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Geek.Core.Utils
{
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
        public static async Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TimeSpan timeout)
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

        //public static async Task WaitAsync(this Task task, TimeSpan timeout)
        //{
        //    using var timeoutCancellationTokenSource = new CancellationTokenSource();
        //    var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
        //    if (completedTask == task)
        //    {
        //        timeoutCancellationTokenSource.Cancel();
        //        await task;  // Very important in order to propagate exceptions
        //    }
        //    else
        //    {
        //        //throw new TimeoutException("The operation has timed out.");
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns>返回是否超时</returns>
        public static async Task<bool> WaitAsync(this Task task, TimeSpan timeout)
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
}
