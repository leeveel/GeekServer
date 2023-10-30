using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Storage;

namespace Geek.Server.App.Logic.Role.Bag
{

    public class BagState : CacheState
    { 
        public Dictionary<int, long> ItemMap = new Dictionary<int, long>();
    }

    [Comp(ActorType.Role)]
    public class BagComp : StateComp<BagState>
    {

    }
}
