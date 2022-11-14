using Server.Logic.Logic;

namespace Geek.Server.Role
{
    public class PetCompAgent : StateCompAgent<PetComp, PetState>
    {

        readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        [Event(EventID.GotNewPet)]
        class EL : EventListener<PetCompAgent>
        {
            protected override async Task HandleEvent(PetCompAgent agent, Event evt)
            {
                switch ((EventID)evt.EventId)
                {
                    case EventID.GotNewPet:
                        await agent.OnGotNewPet((OneParam<int>)evt.Data);
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task OnGotNewPet(OneParam<int> param)
        {
            var serverComp = await ActorMgr.GetCompAgent<ServerCompAgent>();
            //var level = await serverComp.SendAsync(() => serverComp.GetWorldLevel()); //手动入队的写法
            var level = await serverComp.GetWorldLevel();
            LOGGER.Debug($"PetCompAgent.OnGotNewPet监听到了获得宠物的事件,宠物ID:{param.value}当前世界等级:{level}");
        }

    }
}
