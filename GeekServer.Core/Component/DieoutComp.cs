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
                if (t.GetInterface(typeof(IState).FullName) == null)
                    continue;

                //删除数据库中所有StateComponent的document
                var key = "";
                var f = t.GetField("_State");
                if (f != null)
                {
                    key = f.FieldType.FullName;
                }
                else
                {
                    var p = t.GetProperty("_State");
                    if (p != null)
                        key = p.PropertyType.FullName;
                }
                if (string.IsNullOrEmpty(key))
                    continue;

                var col = GetDB().GetCollection<BsonDocument>(key);
                var filter = Builders<BsonDocument>.Filter.Eq(MongoField.Id, ActorId);
                await col.DeleteOneAsync(filter);
            }
        }
    }
}
