using System.Threading.Tasks;

namespace Geek.Server
{
    public interface ITimerHandler
    {
        Task InternalHandleTimer(IComponentAgent actor, Param param);
    }
}
