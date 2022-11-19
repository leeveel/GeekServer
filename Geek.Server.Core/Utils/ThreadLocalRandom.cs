namespace Geek.Server.Core.Utils
{

    /// <summary>
    /// 线程私有random对象
    /// </summary>
    public static class ThreadLocalRandom
    {
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> _rng = new(() => new Random(Interlocked.Increment(ref _seed)));

        /// <summary>
        /// The current random number seed available to this thread
        /// </summary>
        public static Random Current
        {
            get
            {
                return _rng.Value;
            }
        }
    }
}
