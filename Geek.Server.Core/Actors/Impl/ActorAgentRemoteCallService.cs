using Geek.Server.Core.Hotfix;
using MagicOnion;
using MagicOnion.Server;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Core.Actors.Impl
{
    public interface IActorAgentRemoteCallService : IService<IActorAgentRemoteCallService>
    {
        public UnaryResult<ActorRemoteCallResult> Call(ActorRemoteCallParams paras);
    }

    public class ActorAgentRemoteCallService : ServiceBase<IActorAgentRemoteCallService>, IActorAgentRemoteCallService
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public async UnaryResult<ActorRemoteCallResult> Call(ActorRemoteCallParams paras)
        {
            try
            {
                var agentType = HotfixMgr.GetAgentTypeByAgentName(paras.agentName);
                if (agentType == null)
                {
                    return new ActorRemoteCallResult { success = false };
                }
                //目前只针对server级别的actor，否则要改i
                //var agent = await ActorMgr.GetCompAgent(paras.targetActorId, agentType); 
                var agent = await ActorMgr.GetCompAgent(agentType);
                var ret = await agent.RemoteCall(paras);
                return ret;
            }
            catch (Exception ex)
            {
                LOGGER.Error("调用异常:" + ex.Message);
            }
            return null;
        }
    }
}
