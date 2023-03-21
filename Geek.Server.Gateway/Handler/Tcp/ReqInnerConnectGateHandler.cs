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
        public override void Action(NetChannel channel, Message msg)
        {
            var req = msg as ReqInnerConnectGate;
            channel.NodeId = req.NodeId;
            GateNetMgr.ServerConns.Add(channel);
            var res = new ResInnerConnectGate
            {
                IsSuccess = true
            };
            channel.Write(res);
        }
    }
}
