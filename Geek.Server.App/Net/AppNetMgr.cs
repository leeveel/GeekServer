using Geek.Server.Core.Center;
using Geek.Server.Proto;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Geek.Server.App.Net
{
    public class AppNetMgr
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static AppCenterRpcClient CenterRpcClient { get; set; }
        /// <summary>
        /// gateway.nodeid --- InnerTcpClient
        /// </summary>
        private static ConcurrentDictionary<int, InnerTcpClient> tcpClientDic = new ConcurrentDictionary<int, InnerTcpClient>();

        public static Task<bool> ConnectCenter()
        {
            CenterRpcClient = new AppCenterRpcClient(Settings.CenterUrl);
            return CenterRpcClient.Connect();
        }

        public static async Task ConnectGateway()
        {
            var nodes = await CenterRpcClient.ServerAgent.GetNodesByType(NodeType.Gateway);
            foreach (var node in nodes)
            {
                _ = ConnectGateway(node);
            }
        }

        public static async Task ConnectGateway(NetNode node)
        {
            if (node == null || node.Type != NodeType.Gateway)
                return;

            LOGGER.Info($"开始连接网关:{node.NodeId},{node.Ip},{node.TcpPort}");

            if (!tcpClientDic.TryGetValue(node.NodeId, out InnerTcpClient tcpClient))
            {
                tcpClient = new InnerTcpClient(node);
                var res = await tcpClient.Connect();
                if (res)
                {
                    tcpClientDic[node.NodeId] = tcpClient;

                    tcpClient.Register(() =>
                    {
                        var req = new ReqInnerConnectGate();
                        req.NodeId = Settings.ServerId;
                        return req;
                    });
                }
                else
                {
                    LOGGER.Error($"连接网关失败:{node.NodeId},{node.Ip},{node.TcpPort}");
                }
            }
            else
            {
                //对于已有连接，可以立即尝试重连
                tcpClient.TryReConnectImmediately();
            }
        }

        public static async Task GetGlobalConfig()
        {
            //通用配置
            var config = await CenterRpcClient.ServerAgent.GetConfig("global");
            var setting = JsonConvert.DeserializeObject<GlobalSetting>(config.Data);
            if (setting != null)
            {
                Settings.InsAs<BaseSetting>().MongoUrl = setting.MongoUrl;
                Settings.InsAs<BaseSetting>().MonitorUrl = setting.MonitorUrl;
                Settings.InsAs<BaseSetting>().MonitorKey = setting.MonitorKey;
                Settings.InsAs<BaseSetting>().HttpInnerCode = setting.HttpInnerCode;
                Settings.InsAs<BaseSetting>().HttpCode = setting.HttpCode;
                Settings.InsAs<BaseSetting>().LocalDBPrefix = setting.LocalDBPrefix;
                Settings.InsAs<BaseSetting>().LocalDBPath = setting.LocalDBPath;
                Settings.InsAs<BaseSetting>().Language = setting.Language;
                Settings.InsAs<BaseSetting>().SDKType = setting.SDKType;
            }
            else
            {
                LOGGER.Error($"反序列化通用配置失败{setting}");
            }
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
