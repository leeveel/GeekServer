using Common.Net.Tcp;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Net.Tcp.Handler;
using Geek.Server.Gateway.Net;
using Geek.Server.Gateway.Net.Tcp;
using Geek.Server.Gateway.Net.Tcp.Handler;
using Geek.Server.Proto;
using NLog.Fluent;
using System.Diagnostics;

namespace Geek.Server.Gateway.Handler.Tcp
{
    [MsgMapping(typeof(ReqInnerConnectGate))]
    public class ReqInnerConnectGateHandler : BaseHander
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public override void Action(INetChannel channel, Message msg)
        {
            var req = msg as ReqInnerConnectGate;
            channel.NetId = req.NodeId;
            var old = GateNetMgr.AddServerNode(channel);
            if (old != null)
            {
                Log.Info($"内部服务器{req.NodeId} {channel.RemoteAddress}请求连接网关，断开老的连接：{channel.RemoteAddress}");
                old.Close();
            }
            var ids = GateNetMgr.GetAllClientNodeWithTargetId(req.NodeId);
            var res = new ResInnerConnectGate
            {
                IsSuccess = true,
            };
            res.ClientIds = ids;
            _ = channel.Write(new NetMessage(res, 0));
        }
    }
}
