using Geek.Server.Core.Center;

namespace Geek.Server.Gateway.Net.Rpc
{
    public class GateCenterRpcClient : BaseCenterRpcClient
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public override void ConfigChanged(byte[] data)
        {
            LOGGER.Debug("ConfigChanged:" + data);
        }

        public override void NodesChanged(List<NetNode> nodes)
        {
            foreach (var node in nodes)
            {
                LOGGER.Debug("NodesChanged:" + node.NodeId);
            }
            LOGGER.Debug("---------------------------------");
        }
    }
}
