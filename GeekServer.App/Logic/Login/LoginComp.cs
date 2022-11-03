using MessagePack;
using System.Collections.Concurrent;

namespace Geek.Server.Login
{

    [MessagePackObject(true)]
    public class PlayerInfo : InnerState
    {
        //player相对特殊，id不是long，所以不继承DBState，自定义mongoDB的id
        public string playerId;
        public int SdkType;
        public string UserName;

        //这里设定每个账号在1服只有能创建1个角色
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, long> RoleMap = new Dictionary<int, long>();

        [BsonIgnore]
        public bool IsChanged;
    }

    [Comp(ActorType.Server)]
    public class LoginComp : BaseComp
    {
        public ConcurrentDictionary<string, PlayerInfo> PlayerMap = new();
    }
}
