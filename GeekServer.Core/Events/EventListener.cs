using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class EventListener<TAgent> : IEventListener where TAgent : IComponentAgent
    {
        public Task InnerInitListener(ComponentActor actor)
        {
            return InitListener(actor);
        }
        public Task InternalHandleEvent(IComponentAgent agent, Event evt)
        {
            return HandleEvent((TAgent)agent, evt);
        }
        protected abstract Task HandleEvent(TAgent agent, Event evt);
        protected abstract Task InitListener(ComponentActor actor);
    }
}
