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

        public void Close()
        {
            CurDataBase.Close();
        }

        public TState LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            try
            {
                bool isNew = false;
                //读数据
                var state = CurDataBase.GetTable<TState>().Get(id);
                isNew = state == null;
                if (state == null)
                    state = defaultGetter?.Invoke();
                if (state == null)
                    state = new TState { Id = id };
                return state;
            }
            catch (Exception e)
            {
                LOGGER.Fatal(e.ToString());
                return default;
            }
        }

        /// <summary>
        /// 确保在自己线程里面调用
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        public void SaveState<TState>(TState state) where TState : CacheState
        {
            CurDataBase.GetTable<TState>().Set(state.Id, state);
        }

    }
}