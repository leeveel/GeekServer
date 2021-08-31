using System;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    public class NoHotfixComponent : FuncComponent
    {

        public override IComponentAgent GetAgent(Type agentAssemblyType = null)
        {
            if (cacheAgent != null)
                return cacheAgent;
            var agentName = this.GetType().FullName + "Agent";
            var agentType = Type.GetType(agentName);
            if (agentType != null)
            {
                cacheAgent = Activator.CreateInstance(agentType) as IComponentAgent;
                cacheAgent.Owner = this;
            }
            return cacheAgent;
        }

        public override Task Deactive()
        {
            return Task.CompletedTask;
        }

    }
}
