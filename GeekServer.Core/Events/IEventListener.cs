using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IEventListener
    {
        Task InitListener(long entityId);
        Task InnerHandleEvent(IComponentAgent compAgent, Event evt);
    }
}
