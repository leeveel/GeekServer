using System.Threading.Tasks;

namespace Geek.Server
{
    public interface ITimerHandler
    {
        Task InternalHandleTimer(IAgent actor, Param param);
    }
}
