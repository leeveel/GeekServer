
using Geek.Server.Core.Hotfix.Agent;

namespace Geek.Server.Core.Events
{
    public abstract class EventListener<T> : IEventListener where T : ICompAgent
    {
        public Task HandleEvent(ICompAgent agent, Event evt)
        {
            return HandleEvent((T)agent, evt);
        }

        protected abstract Task HandleEvent(T agent, Event evt);


        private readonly Type _agentType = typeof(T);

        public Type AgentType => _agentType;
    }
}
