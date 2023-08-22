using MagicOnion;
using MongoDB.Driver.Core.Servers;

namespace Geek.Server.Core.Center
{
    public interface ICenterRpcClient
    {
        public void ConfigChanged(ConfigInfo data);
        public void ServerChanged(List<ServerInfo> nodes);
        public void HaveMessage(string eid, byte[] msg);
    }

    public interface ICenterRpcHub : IStreamingHub<ICenterRpcHub, ICenterRpcClient>
    {
        public Task<bool> Register(ServerInfo info);
        public Task<ConfigInfo> GetConfig(string configId);
        public Task<List<ServerInfo>> GetAllNodes();
        public Task<List<ServerInfo>> GetNodesByType(ServerType type);
        public Task SyncState(ServerState state);
        public Task Subscribe(string eventId);
        public Task Unsubscribe(string eventId);
    }
}
