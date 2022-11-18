namespace Geek.Server.Role
{

    public class PetState : CacheState
    {

    }

    [Comp(ActorType.Role)]
    public class PetComp : StateComp<PetState>
    {
    }
}
