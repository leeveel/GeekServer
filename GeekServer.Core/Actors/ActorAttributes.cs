using System;

namespace Geek.Server
{

    /// <summary>
    /// Can Be Interleaved when Multi Call Chain Deadlock
    /// 当多条调用链死锁的时候可以被交错
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InterleaveWhenDeadlock : Attribute { }


    public class MethodOption
    {
        ///<summary>此方法将不等待(强制入队执行)</summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class NotAwait : Attribute { };


        ///<summary>此方法线程安全(强制不入队执行)</summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class ThreadSafe : Attribute { };
    }
}
