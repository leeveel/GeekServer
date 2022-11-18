using Geek.Server.Gateway.Logic.Net;
using Geek.Server.Proto;

namespace Geek.Server.Gateway.MessageHandler
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
            conn.WriteAsync(new NMessage(res));
        }
    }
}
