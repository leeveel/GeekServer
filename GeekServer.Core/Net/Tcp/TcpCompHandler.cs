

using System;
using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class TcpCompHandler : BaseTcpHandler
    {
        long cacheEntityId;
        public virtual Task<long> GetEntityId()
        {
            if (cacheEntityId > 0)
                return Task.FromResult(cacheEntityId);
            cacheEntityId = SessionId;
            return Task.FromResult(cacheEntityId);
        }

        public abstract Type CompAgentType { get; }

        protected long SessionId => SessionManager.GetSessionId(Channel);
    }
   
}
