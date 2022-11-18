using Geek.Server.Core.Center;
using Geek.Server.Core.Net.Messages;
using Geek.Server.Core.Net.Tcp.Inner;
using Geek.Server.Proto;
using System.Collections.Concurrent;

namespace Geek.Server.App.Common
{
    public class AppNetMgr
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static CenterRpcClient CenterRpcClient { get; set; }
        /// <summary>
        /// gateway.nodeid --- InnerTcpClient
        /// </summary>
        private static ConcurrentDictionary<int, InnerTcpClient> tcpClientDic = new ConcurrentDictionary<int, InnerTcpClient>();

        public static async Task ConnectCenter()
        {
            CenterRpcClient = new CenterRpcClient();
            await CenterRpcClient.Connect(Settings.CenterUrl);
        }

        public static async Task ConnectGateway(NetNode node)
        {
            if(node == null || node.Type != NodeType.Gateway)
                return;
            if (!tcpClientDic.TryGetValue(node.NodeId, out InnerTcpClient tcpClient))
            {
                tcpClient = new InnerTcpClient(node);
                var res = await tcpClient.Connect(node.Ip, node.InnerTcpPort);
                if (res == NetCode.Success)
                {
                    tcpClientDic[node.NodeId] = tcpClient;
                    var req = new ReqInnerConnectGate();
                    req.NodeId = Settings.ServerId;
                    tcpClient.Write(req);
                }
                else
                {
                    LOGGER.Error($"连接网关失败:{node.NodeId},{node.Ip},{node.TcpPort}");
                }
            }
            else
            {
                LOGGER.Error($"忽略,网关已连接:{node.NodeId},{node.Ip},{node.TcpPort}");
            }
        }


        public static async Task GetCommonConfig()
        {
            //通用配置
            var bytes = await CenterRpcClient.ServerAgent.GetConfig("global");
            
        }

        /// <summary>
        /// 网关服可能有多个
        /// 通过网络节点获取TcpClient
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public static InnerTcpClient GetClientByNodeId(int nodeId)
        {
            tcpClientDic.TryGetValue(nodeId, out InnerTcpClient client);
            return client;
        }

    }
}
