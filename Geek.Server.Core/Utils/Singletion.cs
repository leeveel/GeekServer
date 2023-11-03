public abstract class Singleton<T> where T : Singleton<T>, new()
{
    private static T instance = null;
    private static readonly object lockObject = new();
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance ??= new();
                }
            }
            return instance;
        }
    }
}
