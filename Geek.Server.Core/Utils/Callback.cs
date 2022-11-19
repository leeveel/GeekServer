
using Geek.Server.Core.Hotfix.Agent;

namespace Geek.Server.Core.Utils
{
    public interface IAgentCallback
    {
        Task<bool> Invoke(ICompAgent agent, Param param = null);

        Type CompAgentType();
    }

    public abstract class AgentCallback<TAgent> : IAgentCallback where TAgent : ICompAgent
    {
        public Type CompAgentType()
        {
            return typeof(TAgent);
        }

        public Task<bool> Invoke(ICompAgent agent, Param param = null)
        {
            return OnCall((TAgent)agent, param);
        }

        protected abstract Task<bool> OnCall(TAgent comp, Param param);
    }
}