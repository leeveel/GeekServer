using System;
using System.Threading;

public class SyncContextUtil
{
    public static void Init()
    {
        UnitySynchronizationContext = SynchronizationContext.Current;
        UnityThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    //Unity主线程Id
    public static int UnityThreadId
    {
        get; private set;
    }

    //SynchronizationContext 线程上下文
    //主要有两个方法：
    //  1.Send()：是简单的在当前线程上去调用委托来实现（同步调用）。也就是在子线程上直接调用UI线程执行，等UI线程执行完成后子线程才继续执行。
    //  2.Post()：在线程池上去调用委托来实现（异步调用）。这是子线程会从线程池中找一个线程去调UI线程，子线程不等待UI线程的完成而直接执行自己下面的代码。
    public static SynchronizationContext UnitySynchronizationContext
    {
        get; private set;
    }

    //实现Unity的多线程调度器
    public static void RunOnUnityScheduler(Action action)
    {
        if (SynchronizationContext.Current == SyncContextUtil.UnitySynchronizationContext)
        {
            //本质上就是直接 action()
            //SyncContextUtil.UnitySynchronizationContext.Send(_ => action(), null);
            action();
        }
        else
        {
            SyncContextUtil.UnitySynchronizationContext.Post(_ => action(), null);
        }
    }
}