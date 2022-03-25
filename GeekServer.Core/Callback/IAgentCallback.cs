


using System.Threading.Tasks;

namespace Geek.Server
{
    public interface ICallback
    {
        Task Invoke();
    }

    public interface IAgentCallback
    {
        Task Invoke(long entityId, Param param = null);
    }

    public abstract class AgentCallback<TAgent> : IAgentCallback where TAgent : IComponentAgent
    {
        public virtual async Task Invoke(long entityId, Param param = null)
        {
            var comp = await EntityMgr.GetCompAgent<TAgent>(entityId);
            await comp.Owner.Actor.SendAsync(() => OnInvoke(comp, param));
        }

        protected abstract Task OnInvoke(TAgent agent, Param param);
    }
}
