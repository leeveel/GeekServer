
namespace Geek.Server
{
    public interface ITimerHandler
    {
        Task InnerHandleTimer(ICompAgent actor, Param param);
    }
}
