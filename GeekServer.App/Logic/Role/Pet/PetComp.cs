using MessagePack;

namespace Geek.Server.Role
{

    [MessagePackObject(true)]
    public class PetState : CacheState
    {

    }

    [Comp(ActorType.Role)]
    public class PetComp : StateComp<PetState>
    {
    }
}
