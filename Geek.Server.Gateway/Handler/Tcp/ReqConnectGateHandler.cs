using Common.Net.Tcp;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Net.Tcp.Handler;
using Geek.Server.Gateway.Net;
using Geek.Server.Gateway.Net.Tcp;
using Geek.Server.Gateway.Net.Tcp.Handler;
using Geek.Server.Proto;

namespace Geek.Server.Gateway.Handler.Tcp
{
    [MsgMapping(typeof(ReqConnectGate))]
    public class ReqConnectGateHandler : BaseHander
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override async void Action(INetChannel conn, Message msg)
        {
            var req = msg as ReqConnectGate;
            var node = GateNetMgr.GetServerNode(req.ServerId);
            var res = new ResConnectGate();
            res.UniId = msg.UniId;

            //LOGGER.Error($"设置默认目标节点:{req.ServerId}");
            conn.DefaultTargetNodeId = req.ServerId;
            if (node != null)
            {
                res.Result = true;
                var nmsg = new NetMessage(new ReqClientChannelActive { Address = conn.RemoteAddress }, conn.NetId);
                await node.Write(nmsg);
            }
            Write(conn, res, req.UniId);
        }
    }
}
