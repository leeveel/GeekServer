using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;

namespace Geek.Server.Core.Storage
{
    public enum DBModel
    {
        /// <summary>
        /// 内嵌做主存,mongodb备份
        /// </summary>
        Embeded,
        /// <summary>
        /// mongodb主存,存储失败再存内嵌
        /// </summary>
        Mongodb,
    }

    public interface IGameDB
    {
        public void Open(string url, string dbName);
        public void Close();
        public void Flush(bool wait);
        public Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new();
        public Task SaveState<TState>(TState state) where TState : CacheState;
    }

    public class GameDB
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private static IGameDB dbImpler;


        public static void Init()
        {
            if (Settings.DBModel == (int)DBModel.Embeded)
            {
                dbImpler = new RocksDBConnection();
            }
            else if (Settings.DBModel == (int)DBModel.Mongodb)
            {
                dbImpler = new MongoDBConnection();
            }
            else
            {
                LOGGER.Error($"未知的数据库模式:{Settings.DBModel}");
            }
        }

        public static void Flush(bool wait)
        {
            dbImpler.Flush(wait);
        }

        public static T As<T>() where T : IGameDB
        {
            return (T)dbImpler;
        }

        public static void Open()
        {
            if (Settings.DBModel == (int)DBModel.Embeded)
            {
                dbImpler.Open(Settings.LocalDBPath, Settings.LocalDBPrefix + Settings.ServerId);
            }
            else if (Settings.DBModel == (int)DBModel.Mongodb)
            {
                dbImpler.Open(Settings.MongoUrl, Settings.MongoDBName);
            }
        }

        public static void Close()
        {
            dbImpler.Close();
        }

        public static Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            return dbImpler.LoadState(id, defaultGetter);
        }

        public static Task SaveState<TState>(TState state) where TState : CacheState
        {
            return dbImpler.SaveState(state);
        }


        public static async Task SaveAll()
        {
            if (Settings.DBModel == (int)DBModel.Embeded)
            {
                await ActorMgr.SaveAll();
            }
            else if (Settings.DBModel == (int)DBModel.Mongodb)
            {
                await StateComp.SaveAll();
            }
        }

        public static async Task TimerSave()
        {
            if (Settings.DBModel == (int)DBModel.Embeded)
            {
                await ActorMgr.TimerSave();
            }
            else if (Settings.DBModel == (int)DBModel.Mongodb)
            {
                await StateComp.TimerSave();
            }
        }


    }
}
