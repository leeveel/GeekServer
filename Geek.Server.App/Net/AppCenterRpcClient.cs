using Geek.Server.App.Common;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Actors.Impl;
using Geek.Server.Core.Center;
using Geek.Server.Core.Hotfix;
using MagicOnion;

namespace Geek.Server.App.Net
{
    public class AppCenterRpcClient : BaseCenterRpcClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public AppCenterRpcClient(string ip, int port)
            : base(ip, port)
        {

        }

        public AppCenterRpcClient(string url)
           : base(url)
        {
            ActorRemoteCallHelper.SetGetter((nodeType, actorType) =>
            {
                return this.GetNode(nodeType, actorType);
            }, (typename) =>
            {
                var t = Type.GetType(typename);
                if (t != null)
                    return t;
                return HotfixMgr.HotfixAssembly.GetType(typename);
            });
        }

        public override void ConfigChanged(ConfigInfo data)
        {
            Console.WriteLine("ConfigChanged:" + data);
        }

        public override void NodesChanged(List<NetNode> nodes)
        {
            base.NodesChanged(nodes);
            LOGGER.Debug("---------------------------------");
            foreach (var node in nodes)
            {
                LOGGER.Debug("NodeId:" + node.NodeId);
                if (Settings.InsAs<AppSetting>().ServerReady  //服务器处于ready状态再连接网关
                    && node.Type == NodeType.Gateway)
                {
                    _ = AppNetMgr.ConnectGateway(node);
                }
            }
            LOGGER.Debug("---------------------------------");
        }

        public override void HaveMessage(string eid, byte[] msg)
        {
        }

        public override void RemoteGameServerCallLocalAgent(string callId, byte[] data)
        {

        }
    }
}
