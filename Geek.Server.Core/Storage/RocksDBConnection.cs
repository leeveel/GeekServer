using Geek.Server.Core.Storage.DB;

namespace Geek.Server.Core.Storage
{
    public class RocksDBConnection : IGameDB
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static readonly RocksDBConnection Singleton = new RocksDBConnection();
        public EmbeddedDB CurDataBase { get; private set; }

        public void Open(string url, string dbName)
        {
            CurDataBase = new EmbeddedDB(url + dbName);
        }

        public void Close()
        {
            CurDataBase.Close();
        }

        public void Flush(bool wait)
        {
            CurDataBase.Flush(wait);
        }

        public Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
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

                state.AfterLoadFromDB(isNew);
                return Task.FromResult(state);
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
        public Task SaveState<TState>(TState state) where TState : CacheState
        {
            var (isChanged, data) = state.IsChanged();
            if (isChanged)
            {
                //首先写入版本号，供远程备份进程使用
                state.BeforeSaveToDB();
                CurDataBase.GetTable<TState>().SetRaw(state.Id, data);
                state.AfterSaveToDB();
            }
            return Task.CompletedTask;
        }

    }
}