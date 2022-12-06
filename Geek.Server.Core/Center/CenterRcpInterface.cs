using Geek.Server.Core.Actors.Impl;
using MagicOnion;

namespace Geek.Server.Core.Center
{
    public interface ICenterRpcClient
    {
        public void ConfigChanged(ConfigInfo data);
        public void NodesChanged(List<NetNode> nodes);
        public void HaveMessage(string eid, byte[] msg);
        public void RemoteGameServerCallLocalAgent(string callId, byte[] paras);
    }

    public interface ICenterRpcHub : IStreamingHub<ICenterRpcHub, ICenterRpcClient>
    {
        public Task<bool> Register(NetNode info);
        public Task<ConfigInfo> GetConfig(string configId);
        public Task<List<NetNode>> GetAllNodes();
        public Task<List<NetNode>> GetNodesByType(NodeType type);
        public Task SyncState(NetNodeState state);
        public Task Subscribe(string eventId);
        public Task Unsubscribe(string eventId);
        public Task<ActorRemoteCallResult> ActorAgentCall(int nodeId, byte[] paras);
        public Task SetActorAgentCallResult(string callId, ActorRemoteCallResult result);
    }
}
