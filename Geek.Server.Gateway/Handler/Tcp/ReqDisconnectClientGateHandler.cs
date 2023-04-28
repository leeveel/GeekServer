using Common.Net.Tcp;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Net.Tcp.Handler;
using Geek.Server.Gateway.Net;
using Geek.Server.Gateway.Net.Tcp.Handler;
using Geek.Server.Proto;

namespace Geek.Server.Gateway.Handler.Tcp
{
    [MsgMapping(typeof(ReqDisconnectClient))]
    public class ReqDisconnectClientGateHandler : BaseHander
    {
        public override void Action(INetChannel channel, Message msg)
        {
            var req = msg as ReqDisconnectClient;
            var node = GateNetMgr.GetClientNode(req.TargetNetId);
            if (node != null)
            {
                GateNetMgr.RemoveClientNode(node.NetId);
            }
        }
    }
}
