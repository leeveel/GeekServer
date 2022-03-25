

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

            var channel = GetChannel();
            cacheEntityId = channel.Id;
            return Task.FromResult(cacheEntityId);
        }

        public abstract Type CompAgentType { get; }

        protected Session GetChannel()
        {
            return Channel.GetAttribute(SessionManager.SESSION).Get();
        }
        protected long SessionId => GetChannel().Id;
    }
   
}
