using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class EventListener<TActorAent, TAgent> : IEventListener where TAgent : IAgent where TActorAent : IComponentActorAgent
    {
        public Task InnerInitListener(IAgent owner)
        {
            return InitListener((TActorAent)owner);
        }

        public Task InternalHandleEvent(IAgent agent, Event evt)
        {
            return HandleEvent((TAgent)agent, evt);
        }

        protected abstract Task InitListener(TActorAent actor);
        protected abstract Task HandleEvent(TAgent agent, Event evt);
    }
}
