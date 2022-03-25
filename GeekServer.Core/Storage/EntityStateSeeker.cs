
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class EntityStateSeeker
    {
        readonly static ConcurrentDictionary<long, BsonDocument> stateMap = new ConcurrentDictionary<long, BsonDocument>();
        readonly static NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 玩家没有任何数据在内存时可批量加载其所有State
        /// </summary>
        public static async Task LookUpStates<TRefState>(int entityType, long entityId, bool newPlayer, string levelField = "Level", int minLevel = 50) where TRefState : DBState
        {
            if (stateMap.ContainsKey(entityId))
                return;

            if(newPlayer)
            {
                //新玩家没有任何State需要加载
                var bson = new BsonDocument();
                stateMap[entityId] = bson;
                return;
            }

            var typeList = CompSetting.Singleton.GetAllComps(entityType);
            if (typeList.Count <= 0)
                return;

            try
            {
                var db = MongoDBConnection.Singleton.CurDateBase;
                var col = db.GetCollection<BsonDocument>(typeof(TRefState).FullName);
                var filter = Builders<BsonDocument>.Filter.Eq(MongoField.Id, entityId);

                //低等级不走批量查询
                var baseState = await (await col.FindAsync(filter)).FirstOrDefaultAsync();
                if (baseState == null || baseState.IsBsonNull || baseState.GetValue(levelField, 0).AsInt32 < minLevel)
                    return;

                LOGGER.Warn("lookup加载玩家数据:" + entityId);
                var agg = col.Aggregate().Match(filter);
                IAggregateFluent<BsonDocument> af = null;
                for (int i = 0; i < typeList.Count; ++i)
                {
                    var type = typeList[i];
                    if (type.GetInterface(typeof(IState).FullName) == null)
                        continue;
                    var arr = type.BaseType.GetGenericArguments();
                    if (arr.Length <= 0)
                        continue;
                    var sType = arr[0];
                    if (!sType.IsSubclassOf(typeof(DBState)))
                        continue;

                    if (af == null)
                        af = agg.Lookup(sType.FullName, MongoField.Id, MongoField.Id, sType.FullName.Replace(".", "_"));
                    else
                        af = af.Lookup(sType.FullName, MongoField.Id, MongoField.Id, sType.FullName.Replace(".", "_"));
                }
                if (af == null)
                    return;
                var doc = await af.FirstOrDefaultAsync();
                if (doc == null)
                    doc = new BsonDocument();
                stateMap[entityId] = doc;
            }
            catch (System.Exception e)
            {
                LOGGER.Error(e.ToString());
            }
        }

        public static Task ClearStates(long entityId)
        {
            stateMap.TryRemove(entityId, out var _);
            return Task.CompletedTask;
        }

        public static TState GetState<TState>(long actorId) where TState : DBState, new()
        {
            if (!stateMap.TryGetValue(actorId, out var doc))
                return default;
            var stateName = typeof(TState).FullName.Replace(".", "_");
            var state = doc.GetValue(stateName, null);
            TState t = default;
            if (state == null || state.IsBsonNull)
            {
                t = new TState() { Id = actorId };
            }
            else
            {
                if (state.IsBoolean)
                    return default;

                var arr = state.AsBsonArray;
                if (arr.Count > 0)
                    t = BsonSerializer.Deserialize<TState>(arr[0].AsBsonDocument);
                else
                    t = new TState() { Id = actorId };

                doc.Set(stateName, new BsonBoolean(false));
            }
            t.ClearChanges();
            return t;
        }
    }
}