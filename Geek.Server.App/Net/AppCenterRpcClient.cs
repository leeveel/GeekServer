using Geek.Server.App.Common;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Actors.Impl;
using Geek.Server.Core.Center;
using Geek.Server.Core.Hotfix;

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
            ActorRemoteCall.SetRpc(this, (typename) =>
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
            LOGGER.Info("RemoteGameServerCallLocalAgent.........................");
            _ = Task.Run(async () =>
            {
                try
                {
                    var paras = MessagePack.MessagePackSerializer.Deserialize<ActorRemoteCallParams>(data);
                    var agentType = HotfixMgr.GetAgentTypeByAgentName(paras.agentName);

                    if (agentType == null)
                    {
                        await ServerAgent.SetActorAgentCallResult(callId, new ActorRemoteCallResult { success = false });
                        return;
                    }
                    //目前只针对server级别的actor，否则要改i
                    //var agent = await ActorMgr.GetCompAgent(paras.targetActorId, agentType); 
                    var agent = await ActorMgr.GetCompAgent(agentType);
                    var ret = await agent.RemoteCall(paras);
                    await ServerAgent.SetActorAgentCallResult(callId, ret);
                    LOGGER.Debug("调用结束，设置结果:" + callId);
                }
                catch (Exception ex)
                {
                    LOGGER.Error("调用异常:" + ex.Message);
                }
            });
        }
    }
}
