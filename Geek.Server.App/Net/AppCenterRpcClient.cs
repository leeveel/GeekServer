using Geek.Server.Core.Center;

namespace Geek.Server.App.Net
{
    public class AppCenterRpcClient : BaseCenterRpcClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public override void ConfigChanged(byte[] data)
        {
            Console.WriteLine("ConfigChanged:" + data);
        }

        public override void NodesChanged(List<NetNode> nodes)
        {
            foreach (var node in nodes)
            {
                LOGGER.Debug("NodesChanged:" + node.NodeId);
                if (node.Type == NodeType.Gateway)
                {
                    _ = AppNetMgr.ConnectGateway(node);
                }
            }
            LOGGER.Debug("---------------------------------");
        }
    }
}
