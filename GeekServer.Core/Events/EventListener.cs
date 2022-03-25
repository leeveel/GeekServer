using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class EventListener<TAgent> : IEventListener where TAgent : IComponentAgent
    {
        public abstract Task InitListener(long entityId);
        public Task InnerHandleEvent(IComponentAgent agent, Event evt)
        {
            return HandleEvent((TAgent)agent, evt);
        }
        protected abstract Task HandleEvent(TAgent agent, Event evt);
    }
}
