
namespace Geek.Server
{
    public abstract class StateCompAgent<TComp, TState> : BaseCompAgent<TComp> where TComp : StateComp<TState> where TState : CacheState, new()
    {
        public TState State => Comp.State;
    }
}
