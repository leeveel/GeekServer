using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class TimerHandler<T> : ITimerHandler where T : IAgent
    {
        public Task InternalHandleTimer(IAgent agent, Param param)
        {
            return HandleTimer((T)agent, param);
        }

        protected abstract Task HandleTimer(T agent, Param param);
    }
}
