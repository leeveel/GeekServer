namespace Geek.Server
{
    public class RocksDBConnection
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static readonly RocksDBConnection Singleton = new RocksDBConnection();
        public EmbeddedDB CurDataBase { get; private set; }
        public void Connect(string db)
        {
            CurDataBase = new EmbeddedDB(db);
        }

        public ValueTask<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            try
            {
                bool isNew=false;
                //读数据
                var state = CurDataBase.GetTable<TState>().Get(id);
                isNew = state == null;
                if (state == null)
                    state = defaultGetter?.Invoke();
                if (state == null)
                    state = new TState { Id = id };
                state.AfterLoadFromDB(isNew);
                return new ValueTask<TState>(state);
            }
            catch (Exception e)
            {
                LOGGER.Fatal(e.ToString());
                return default;
            }
        }

        public ValueTask SaveState<TState>(TState state) where TState : CacheState
        {
            var (isChanged, data) = state.IsChanged();
            if (isChanged)
            {
                var _state = Serializer.Deserialize<TState>(data);
                CurDataBase.GetTable<TState>().Set(_state.Id, _state);
                state.AfterSaveToDB();
            }
            return ValueTask.CompletedTask;
        }

    }
}