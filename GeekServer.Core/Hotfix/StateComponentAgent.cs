using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class StateComponentAgent<TComp, TState> : BaseComponentAgent<TComp> where TComp : BaseComponent, IState  where TState : DBState
    {
        public TState State => (TState)((IState)Owner).State;

        public Task WriteStateAsync()
        {
            return ((IState)Owner).WriteStateAsync();
        }

        public Task ReloadState(int coldTimeInMinutes = 30)
        {
            return ((IState)Owner).ReloadState(coldTimeInMinutes);
        }
    }
}