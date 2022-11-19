
using Geek.Server.Core.Hotfix.Agent;
using Geek.Server.Core.Utils;

namespace Geek.Server.Core.Timer.Handler
{
    public interface ITimerHandler
    {
        Task InnerHandleTimer(ICompAgent actor, Param param);
    }
}
