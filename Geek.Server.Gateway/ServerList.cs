using Geek.Server.Core.Center;
using Geek.Server.Gateway.Common;
using MongoDB.Driver.Core.Servers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.GatewayKcp
{
    internal class ServerList : BaseCenterRpcClient
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        static ServerList _instance;
        public static ServerList Instance
        {
            get
            {
                _instance ??= new ServerList();
                return _instance;
            }
        }

        ConcurrentDictionary<long, ServerInfo> nodeMap = new();

        public ServerList() : base(Settings.CenterUrl)
        {
            _instance = this;
        }


        public async Task Start()
        {
            if (await Connect())
            {
                var infoGetter = () => new ServerInfo
                {
                    ServerId = Settings.ServerId,
                    Ip = Settings.LocalIp,
                    TcpPort = Settings.TcpPort,
                    HttpPort = Settings.HttpPort,
                    Type = ServerType.Gate,
                };

                var stateGetter = () =>
                {
                    var state = new Core.Center.ServerState();
                    state.MaxLoad = 20000;
                    state.CurrentLoad = 1;
                    return state;
                };

                //上报注册中心
                if (!await Register(infoGetter, stateGetter))
                    throw new Exception($"中心服注册失败... {MessagePack.MessagePackSerializer.SerializeToJson(infoGetter())}");
            }
            else
            {
                throw new Exception($"连接中心服失败...");
            }
        }

        public override void ConfigChanged(ConfigInfo data)
        {
            LOGGER.Debug("ConfigChanged:" + data);
        }

        public override void HaveMessage(string eid, byte[] msg)
        {
        }

        public override void ServerChanged(List<ServerInfo> nodes)
        {
            nodeMap.Clear();
            LOGGER.Debug("---------------------------------");
            foreach (var node in nodes)
            {
                LOGGER.Debug("NodeId:" + node.ServerId);
                if (node.Type == ServerType.Game)
                    node.InnerUdpEndPoint = new IPEndPoint(IPAddress.Parse(node.InnerIp), node.TcpPort);
                nodeMap[node.ServerId] = node;
            }
            LOGGER.Debug("---------------------------------");

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
