using Geek.Server.Core.Discovery;
using MagicOnion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Discovery
{
    public interface IDiscoveryHub : IStreamingHub<IDiscoveryHub, IDiscoveryClient>
    {
        public Task<bool> Register(ServerInfo info);
        public Task<List<ServerInfo>> GetAllNodes();
        public Task<List<ServerInfo>> GetNodesByType(ServerType type);
        public Task SyncState(ServerState state);
    }
}
