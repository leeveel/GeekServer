using Geek.Server.Proto;
using Geek.Server.Gateway.MessageHandler;
using Geek.Server.Gateway.Net.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Gateway.MessageHandler
{
    [MsgMapping(typeof(ReqRouterMsg))]
    public class ReqRouterMsgHandler : BaseHander
    {
        public override void Run(INetNode node, Message msg)
        {
            var req = msg as ReqRouterMsg;
            var targetNode = NetNodeMgr.Get(req.TargetUid);
            var res = new ResRouterMsg();
            if (targetNode == null)
            {
                res.Result = false;
                WriteMsgWithState(node, res, msg.UniId);
                node.OnTargetNotExist();
                return;
            }
            else
            {
                res.Result = true;
                node.defaultTargetUid = req.TargetUid;
                WriteMsgWithState(node, res, msg.UniId);
                MsgRouter.NodeReqRouter(node, req.TargetUid);
            }
        }
    }
}
