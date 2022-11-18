using Geek.Server.Core.Hotfix.Agent;

namespace Geek.Server.Core.Timer.Handler
{
    public interface ITimerHandler
    {
        Task InnerHandleTimer(ICompAgent actor, Param param);
    }
}
