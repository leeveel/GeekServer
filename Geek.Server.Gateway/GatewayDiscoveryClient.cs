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

        readonly ConcurrentDictionary<long, ServerInfo> allServerMap = new();
        readonly ConcurrentDictionary<long, ServerInfo> gatewayServerMap = new();

        private readonly string whenGateStateChangeEvtId = SubscribeEvent.NetNodeStateChange(ServerType.Gate);

        public GatewayDiscoveryClient() :
            base(Settings.Ins.DiscoveryServerUrl,
                () => new ServerInfo
                {
                    ServerId = Settings.Ins.ServerId,
                    PublicIp = Settings.InsAs<GateSettings>().PublicIp,
                    LocalIp = Settings.Ins.LocalIp,
                    InnerPort = Settings.Ins.InnerPort,
                    OuterPort = Settings.Ins.OuterPort,
                    HttpPort = Settings.Ins.HttpPort,
                    Type = ServerType.Gate,
                },
                () => new ServerState
                {
                    MaxLoad = Settings.InsAs<GateSettings>().MaxClientCount,
                    CurrentLoad = GateServer.Instance.CurActiveChannelCount
                })
        {
        }

        public override void OnRegister()
        {
            ServerAgent?.Subscribe(whenGateStateChangeEvtId);
        }

        public override void HaveMessage(string eid, byte[] msg)
        {
            if (whenGateStateChangeEvtId == eid)
            {
                var info = MessagePack.MessagePackSerializer.Deserialize<ServerInfo>(msg);
                if (info != null)
                {
                    var oldInfo = GetServer(info.ServerId);
                    if (oldInfo != null)
                    {
                        oldInfo.State = info.State;
#if DEBUG
                        LOGGER.Debug($"更新server[{info.ServerId}][{info.Type}]状态,{info.State?.ToString()}");
#endif
                    }
                }
            }
        }

        public override void ServerChanged(List<ServerInfo> nodes)
        {
            allServerMap.Clear();
            gatewayServerMap.Clear();
            LOGGER.Debug("ServerChanged:" + nodes.Count);
            foreach (var node in nodes)
            {
                try
                {
#if DEBUG
                    LOGGER.Debug(MessagePack.MessagePackSerializer.SerializeToJson(node));
#endif
                    if (node.Type == ServerType.Game)
                    {
                        if (string.IsNullOrEmpty(node.LocalIp))
                        {
                            node.InnerEndPoint = null;
                            LOGGER.Error($"Logic server [{node.ServerId}] localIp is empty...");
                        }
                        else
                        {
                            node.InnerEndPoint = new IPEndPoint(IPAddress.Parse(node.LocalIp), node.InnerPort);
                        }
                    }
                    else if (node.Type == ServerType.Gate)
                    {
                        gatewayServerMap[node.ServerId] = node;
                    }
                    allServerMap[node.ServerId] = node;
                }
                catch (Exception e)
                {
                    LOGGER.Error(e);
                }
            }
        }

        public ServerInfo GetServer(int serverId, ServerType type)
        {
            if (allServerMap.TryGetValue(serverId, out var server))
            {
                if (server.Type == type)
                    return server;
            }
            return null;
        }

        public ServerInfo GetServer(int serverId)
        {
            if (allServerMap.TryGetValue(serverId, out var server))
            {
                return server;
            }
            return null;
        }

        public ServerInfo GetIdleGateway()
        {
            foreach (var kv in gatewayServerMap)
            {
                var sInfo = kv.Value;
                if (sInfo.State.CurrentLoad < sInfo.State.MaxLoad)
                {
                    return sInfo;
                }
            }
            return null;
        }
    }
}
