using Geek.Server.Core.Actors;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Storage;

namespace Geek.Server.App.Logic.Role.Pet
{

    public class PetState : CacheState
    {

    }

    [Comp(ActorType.Role)]
    public class PetComp : StateComp<PetState>
    {
    }
}
