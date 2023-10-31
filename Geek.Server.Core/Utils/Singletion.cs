using System;
using System.Linq;
using System.Reflection;

public abstract class Singleton<T> where T : Singleton<T>
{
    private const string ErrorMessage = "单例构造函数不能为public";
    private static T instance = null;
    private static readonly object lockObject = new Object();
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance ??= Create();
                }
            }
            return instance;
        }
    }

    protected Singleton()
    {
        var pconstr = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        if (pconstr.Any())
            throw new Exception(typeof(T) + ErrorMessage);
    }

    private static T Create()
    {
        try
        {
            var constructors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)constructors.Single().Invoke(null);
        }
        catch
        {
            throw new Exception(typeof(T) + ErrorMessage);
        }
    }
}
