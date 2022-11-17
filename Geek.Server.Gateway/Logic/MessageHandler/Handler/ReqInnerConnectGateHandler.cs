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
            //conn.TargetId = req.NodeId;
            conn.Id = req.NodeId;
            GateNetHelper.ServerConns.Add(conn);
            var res = new ResInnerConnectGate
            {
                IsSuccess = true
            };
            conn.Channel.WriteAsync(new NMessage(res));
        }
    }
}
