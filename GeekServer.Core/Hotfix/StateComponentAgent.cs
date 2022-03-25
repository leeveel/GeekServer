


namespace Geek.Server
{
    public abstract class StateComponentAgent<TComp, TState> : BaseComponentAgent<TComp> where TComp : BaseComponent, IState  where TState : DBState
    {
        public TState State => (TState)((IState)Owner).State;
    }
}