namespace Geek.Server.Core.Storage
{
    public interface IGameDB
    {
        public void Open(string url, string dbName);
        public void Close();
        public Task Flush();
        public Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new();
        public Task SaveState<TState>(TState state) where TState : CacheState;
    }

    public class GameDB
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private static IGameDB dbImpler;


        public static void Init()
        {
            dbImpler = new MongoDBConnection();
        }

        public static async Task Flush()
        {
            await dbImpler.Flush();
        }

        public static T As<T>() where T : IGameDB
        {
            return (T)dbImpler;
        }

        public static void Open()
        {
            dbImpler.Open(Settings.MongoUrl, Settings.MongoDBName);
        }

        public static void Close()
        {
            dbImpler.Close();
        }

        public static Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            return dbImpler.LoadState(id, defaultGetter);
        }

        public static async Task SaveState<TState>(TState state) where TState : CacheState
        {
            await dbImpler.SaveState(state);
        }
    }
}
