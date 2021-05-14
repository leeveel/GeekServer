using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class DieoutComp : QueryComponent
    {
        public async Task Dieout(List<Type> compTypeList)
        {
            foreach (var t in compTypeList)
            {
                var key = "";
                var p = t.GetProperty("_State");
                if (p != null)
                {
                    key = p.PropertyType.FullName;
                }
                else
                {
                    var f = t.GetField("_State");
                    if (f != null)
                        key = f.FieldType.FullName;
                }
                if (string.IsNullOrEmpty(key))
                    continue;

                var col = GetDB().GetCollection<BsonDocument>(key);
                var filter = Builders<BsonDocument>.Filter.Eq(MongoField.UniqueId, ActorId);
                await col.DeleteOneAsync(filter);
            }
        }
    }
}
