using Geek.Server.Gateway.Logic.Net;
using Geek.Server.Proto;

namespace Geek.Server.Gateway.MessageHandler
{
    [MsgMapping(typeof(ReqConnectGate))]
    public class ReqConnectGateHandler : BaseHander
    {

        public override void Action(Connection conn, Message msg)
        {
            var req = msg as ReqConnectGate;
            //分配网络节点(选择一个负载较小的节点)
            var nodeId = GateNetMgr.SelectAHealthNode(req.ServerId);
            if (nodeId > 0)
            {
                //设置client connection的targetid
                conn.TargetId = nodeId;
                var res = new ResConnectGate
                {
                    ServerId = req.ServerId,
                    NodeId = nodeId
                };
                WriteWithStatus(conn, res, req.UniId);
            }
            else
            {
                //未找到ServerId对应的网络节点
                var res = new NodeNotFound
                {
                    NodeId = req.ServerId
                };
                conn.Channel.WriteAsync(new NMessage(res));
            }
        }

    }
}
