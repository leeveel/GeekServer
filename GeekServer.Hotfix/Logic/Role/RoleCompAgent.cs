using System.Threading.Tasks;

namespace Geek.Server.Logic.Role
{
    public class RoleCompAgent : StateComponentAgent<RoleComp, RoleState>
    {
        class EH : EventListener<RoleCompAgent>
        {
            protected override async Task HandleEvent(RoleCompAgent agent, Event evt)
            {
                switch (evt.EventId)
                {
                    case (int)EventID.OnDisconnected:
                        await agent.OnDisconnected();
                        break;
                    case (int)EventID.OnMsgReceived:
                        await agent.OnMsgReceived();
                        break;
                }
            }

            protected override Task InitListener(ComponentActor actor)
            {
                actor.EvtDispatcher.AddListener(EventID.OnDisconnected, this);
                actor.EvtDispatcher.AddListener(EventID.OnMsgReceived, this);
                return Task.CompletedTask;
            }
        }

        public Task<bool> IsOnline()
        {
            return Task.FromResult(true);
        }

        public Task OnDisconnected()
        {
            return Task.CompletedTask;
        }

        public Task OnMsgReceived()
        {
            return Task.CompletedTask;
        }

    }
}
