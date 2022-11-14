namespace Geek.Server.Role
{

    public class BagState : CacheState
    {
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, long> ItemMap = new Dictionary<int, long>();
    }

    [Comp(ActorType.Role)]
    public class BagComp : StateComp<BagState>
    {

    }
}
