using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;

namespace Geek.Server.Logic.Login
{

    public class PlayerInfoState : BaseDBState
    {
        //player相对特殊，id不是long，所以不继承DBState，自定义mongoDB的id
        public string Id { get; set; }
        public int SdkType { get; set; }
        public string UserName { get; set; }

        //这里设定每个账号在1服只有能创建1个角色
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public StateMap<int, long> RoleMap { get; set; } = new StateMap<int, long>();
    }

    public class LoginComp : QueryComponent
    {
        //仅在内存中缓存
        public Dictionary<string, PlayerInfoState> PlayerMap = new Dictionary<string, PlayerInfoState>();
    }
}
