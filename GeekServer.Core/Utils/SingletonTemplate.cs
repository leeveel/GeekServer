

namespace Geek.Server
{
    public class SingletonTemplate<T> where T : class, new()
    {
        public static readonly T Singleton = new T();
    }
}

