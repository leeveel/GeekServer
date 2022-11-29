using Geek.Server.Core.Center;

namespace Geek.Server.Rebalance.Net.Rpc
{
    public class CenterRpcClient : BaseCenterRpcClient
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public CenterRpcClient(string url) : base(url)
        {
        }

        public override void ConfigChanged(ConfigInfo data)
        {
            LOGGER.Debug("ConfigChanged:" + data);
        }


        public override void HaveMessage(string eid, byte[] msg)
        {
            if (eid.EndsWith(SubscribeEvent.NetNodeStateChangeSuffix))
            {
                GatewayMgr.UpdateNode(MessagePack.MessagePackSerializer.Deserialize<NetNode>(msg));
            }
        }

        public override void NodesChanged(List<NetNode> nodes)
        {
            LOGGER.Debug("---------------------------------");
            foreach (var node in nodes)
            {
                LOGGER.Debug("NodeId:" + node.NodeId);
            }

            GatewayMgr.ResetAllNode(nodes.FindAll((node) =>
            {
                return node.Type == NodeType.Gateway;
            }));
            LOGGER.Debug("---------------------------------");
        }
    }
}
