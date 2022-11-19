using System.Collections.Concurrent;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Geek.Server.Core.Storage
{
    public class RoleStateLoader
    {
        private const string KEY = CacheState.UniqueId;

        readonly static ConcurrentDictionary<long, BsonDocument> stateMap = new ConcurrentDictionary<long, BsonDocument>();
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 玩家没有任何数据在内存时可批量加载其所有State
        /// </summary>
        public static async Task LookUpStates<TState>(ActorType actorType, long actorId, bool newPlayer, string levelField = "Level", int minLevel = 50) where TState : CacheState
        {
            if (stateMap.ContainsKey(actorId))
                return;

            if (newPlayer)
            {
                //新玩家没有任何State需要加载
                var bson = new BsonDocument();
                stateMap[actorId] = bson;
                return;
            }

            var typeList = CompRegister.GetComps(actorType);
            if (typeList == null)
                return;

            var db = MongoDBConnection.CurDB;
            var col = db.GetCollection<BsonDocument>(typeof(TState).FullName);
            var filter = Builders<BsonDocument>.Filter.Eq(KEY, actorId);

            //低等级不走批量查询
            var baseState = await (await col.FindAsync(filter)).FirstOrDefaultAsync();
            if (baseState == null || baseState.IsBsonNull || baseState.GetValue(levelField, 0).AsInt32 < minLevel)
                return;

            LOGGER.Warn("lookup加载玩家数据:" + actorId);
            var agg = col.Aggregate().Match(filter);
            IAggregateFluent<BsonDocument> af = null;
            foreach (var type in typeList)
            {
                if (type.GetInterface(typeof(IState).FullName) == null)
                    continue;
                var arr = type.BaseType.GetGenericArguments();
                if (arr.Length <= 0)
                    continue;
                var sType = arr[0];
                if (!sType.IsSubclassOf(typeof(CacheState)))
                    continue;

                if (af == null)
                    af = agg.Lookup(sType.FullName, KEY, KEY, sType.FullName.Replace(".", "_"));
                else
                    af = af.Lookup(sType.FullName, KEY, KEY, sType.FullName.Replace(".", "_"));
            }
            if (af == null)
                return;
            var doc = await af.FirstOrDefaultAsync();
            if (doc == null)
                doc = new BsonDocument();
            stateMap[actorId] = doc;
        }

        public static Task ClearStates(long actorId)
        {
            stateMap.TryRemove(actorId, out var _);
            return Task.CompletedTask;
        }

        public static TState GetState<TState>(long actorId) where TState : CacheState, new()
        {
            if (!stateMap.TryGetValue(actorId, out var doc))
                return default;
            string stateName = typeof(TState).FullName.Replace(".", "_");
            var state = doc.GetValue(stateName, null);
            TState t = default;
            if (state == null || state.IsBsonNull)
            {
                t = new TState() { Id = actorId };
            }
            else if (state.IsBoolean)
            {
                return default;
            }
            else
            {
                var arr = state.AsBsonArray;
                if (arr.Count > 0)
                    t = BsonSerializer.Deserialize<TState>(arr[0].AsBsonDocument);
                else
                    t = new TState() { Id = actorId };
                doc.Set(stateName, new BsonBoolean(false));
            }
            return t;
        }
    }
}