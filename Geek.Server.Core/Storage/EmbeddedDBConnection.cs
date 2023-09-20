using Core.Storage.DB;
using Geek.Server.Core.Storage;

namespace Core.Storage
{
    public class EmbeddedDBConnection : IGameDB
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static readonly EmbeddedDBConnection Singleton = new();
        public EmbeddedDB CurDataBase { get; private set; }

        public void Open(string url, string dbName)
        {
            CurDataBase = new EmbeddedDB(Path.Combine(url, dbName));
            CurDataBase.InnerDB.UnderlyingDatabase.Mapper.RegisterIgnoreAttribute(typeof(MongoDB.Bson.Serialization.Attributes.BsonIgnoreAttribute));
        }

        public void Close()
        {
            CurDataBase.Close();
        }

        public async Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            try
            {
                bool isNew = false;
                //读数据
                var state = await CurDataBase.GetTable<TState>().Get(id);
                isNew = state == null;
                state ??= defaultGetter?.Invoke();
                state ??= new TState { Id = id };

                state.AfterLoadFromDB(isNew);
                return state;
            }
            catch (Exception e)
            {
                LOGGER.Fatal($"LoadState:{typeof(TState).FullName} [{id}] {e}");
                return default;
            }
        }

        public async Task<TState> LoadState<TState>(string id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            try
            {
                bool isNew = false;
                //读数据
                var state = await CurDataBase.GetTable<TState>().Get(id);
                isNew = state == null;
                state ??= defaultGetter?.Invoke();
                state.AfterLoadFromDB(isNew);
                return state;
            }
            catch (Exception e)
            {
                LOGGER.Fatal($"LoadState:{typeof(TState).FullName} [{id}] {e.ToString()}");
                return default;
            }
        }

        /// <summary>
        /// 确保在自己线程里面调用
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        public async Task SaveState<TState>(TState state) where TState : CacheState
        {
            await CurDataBase.GetTable<TState>().Set(state.Id, state);
            state.AfterSaveToDB();
        }


        public async Task Flush()
        {
            await CurDataBase.Flush();
        }
    }
}
