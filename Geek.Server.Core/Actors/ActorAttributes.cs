namespace Geek.Server.Core.Actors
{

    /// <summary>
    /// 此方法会提供给其他Actor访问
    /// </summary>
    [Obsolete("过时的,请用[Service]代替")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class Api : Attribute { };

    /// <summary>
    /// 此方法会提供给其他Actor访问(对外提供服务)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class Service : Attribute { };


    ///<summary>此方法线程安全</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ThreadSafe : Attribute { };

    /// <summary>
    /// 此方法使用了弃元运算符，不会等待执行(将强制追加到队列末端执行)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class Discard : Attribute { };

    /// <summary>
    /// 超时时间(毫秒)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TimeOut : Attribute { public TimeOut(int timeout) { } }
}
