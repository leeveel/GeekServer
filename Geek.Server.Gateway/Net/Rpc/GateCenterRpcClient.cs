using Geek.Server.Core.Center;

namespace Geek.Server.Gateway.Net.Rpc
{
    public class GateCenterRpcClient : BaseCenterRpcClient
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public GateCenterRpcClient(string url) : base(url)
        {
        }

        public override void ConfigChanged(ConfigInfo data)
        {
            LOGGER.Debug("ConfigChanged:" + data);
        }

        public override void HaveMessage(string eid, string msg)
        {
        }

        public override void NodesChanged(List<NetNode> nodes)
        {
            LOGGER.Debug("---------------------------------");
            foreach (var node in nodes)
            {
                LOGGER.Debug("NodeId:" + node.NodeId);
            }
            LOGGER.Debug("---------------------------------");
        }
    }
}
