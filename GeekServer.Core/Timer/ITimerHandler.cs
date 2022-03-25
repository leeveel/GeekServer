

using System.Threading.Tasks;

namespace Geek.Server
{
    public interface ITimerHandler
    {
        Task InnerHandleTimer(IComponentAgent actor, Param param);
    }
}
