using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IEventListener
    {
        Task InnerInitListener(IAgent actor);

        Task InternalHandleEvent(IAgent actor, Event evt);
    }
}
