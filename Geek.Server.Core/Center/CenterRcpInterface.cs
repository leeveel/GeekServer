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
        public Task<byte[]> GetConfig(string configId);
        public Task<bool> SetConfig(string configId, byte[] data);
        public Task<List<NetNode>> GetAllNodes();
        public Task<List<NetNode>> GetNodeByType(NodeType type);
    }
}
