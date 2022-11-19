using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Net.Tcp.Handler;
using Geek.Server.Gateway.Net;
using Geek.Server.Gateway.Net.Tcp.Handler;
using Geek.Server.Proto;

namespace Geek.Server.Gateway.Handler.Tcp
{
    [MsgMapping(typeof(ReqInnerConnectGate))]
    public class ReqInnerConnectGateHandler : BaseHander
    {
        public override void Action(Connection conn, Message msg)
        {
            var req = msg as ReqInnerConnectGate;
            GateNetMgr.ServerConns.SetNodeId(conn, req.NodeId);
            var res = new ResInnerConnectGate
            {
                IsSuccess = true
            };
            conn.WriteAsync(new NetMessage(res));
        }
    }
}
