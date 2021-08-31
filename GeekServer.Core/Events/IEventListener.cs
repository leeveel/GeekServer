using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IEventListener
    {
        Task InnerInitListener(ComponentActor actor);
        Task InternalHandleEvent(IComponentAgent compAgent, Event evt);
    }
}
