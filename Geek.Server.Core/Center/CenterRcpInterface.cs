using MagicOnion;

namespace Geek.Server.Core.Center
{
    public interface ICenterRpcClient
    {
        public void ConfigChanged(byte[] data);

        public void NodesChanged(List<NetNode> nodes);
    }

    public interface ICenterRpcHub : IStreamingHub<ICenterRpcHub, ICenterRpcClient>
    {
        public Task<bool> Register(NetNode info);
        public Task<ConfigInfo> GetConfig(string configId);
        public Task<bool> SetConfig(ConfigInfo data);
        public Task<List<NetNode>> GetAllNodes();
        public Task<List<NetNode>> GetNodeByType(NodeType type);
    }
}
