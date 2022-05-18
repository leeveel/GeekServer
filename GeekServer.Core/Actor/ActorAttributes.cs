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


        ///<summary>此方法执行时间较长(增加超时时间(默认10秒，增加到20秒))</summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class LongTimeTake : Attribute { };


        ///<summary>此方法会被同名组件调用(强制非public判断入队)</summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class CanBeCalledBySameComp : Attribute { };


        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class ExecuteTime : Attribute 
        {
            public int Time { get; private set; }
            public ExecuteTime(int time)
            {
                Time = time;
            }
        }

    }
}
