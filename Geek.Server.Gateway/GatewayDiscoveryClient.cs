using Core.Discovery;
using Geek.Server.Core.Discovery;
using Geek.Server.Gateway.Common;
using System.Collections.Concurrent;
using System.Net;

namespace Geek.Server.Gateway
{
    internal class GatewayDiscoveryClient : BaseDiscoveryClient
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        static GatewayDiscoveryClient _instance;
        public static GatewayDiscoveryClient Instance
        {
            get
            {
                _instance ??= new GatewayDiscoveryClient();
                return _instance;
            }
        }

        readonly ConcurrentDictionary<long, ServerInfo> nodeMap = new();

        public GatewayDiscoveryClient() :
            base(Settings.Ins.DiscoveryServerUrl,
                () => new ServerInfo
                {
                    ServerId = Settings.Ins.ServerId,
                    Ip = Settings.Ins.LocalIp,
                    localIp = Settings.Ins.LocalIp,
                    InnerPort = Settings.Ins.InnerPort,
                    OuterPort = Settings.Ins.OuterPort,
                    HttpPort = Settings.Ins.HttpPort,
                    Type = ServerType.Gate,
                },
                () => new ServerState
                {
                    MaxLoad = Settings.InsAs<GateSettings>().MaxClientCount,
                    CurrentLoad = GateServer.Instance.curActiveChannelCount
                })
        {
        }

        public override void HaveMessage(string eid, byte[] msg)
        {
        }

        public override void ServerChanged(List<ServerInfo> nodes)
        {
            nodeMap.Clear();
            LOGGER.Debug("ServerChanged:" + nodes.Count);
            foreach (var node in nodes)
            {
                try
                {
                    if (node.Type == ServerType.Game)
                    {
                        if (string.IsNullOrEmpty(node.localIp))
                        {
                            LOGGER.Error($"Logic server [{node.ServerId}] localIp is empty...");
                        }
                        else
                        {
                            node.InnerEndPoint = new IPEndPoint(IPAddress.Parse(node.localIp), node.InnerPort);
                        }
                    }
                    nodeMap[node.ServerId] = node;
                }
                catch (Exception e)
                {
                    LOGGER.Error(e);
                }
            }
        }

        public ServerInfo GetServer(int serverId, ServerType type)
        {
            if (nodeMap.TryGetValue(serverId, out var server))
            {
                if (server.Type == type)
                    return server;
            }
            return null;
        }
    }
}
