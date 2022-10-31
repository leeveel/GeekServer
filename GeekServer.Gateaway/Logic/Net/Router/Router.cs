using GeekServer.Gateaway.MessageHandler;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.Net.Router
{
    public class MsgRouter
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void NodeReqRouter(INetNode node, long targetId)
        {
            if (node.type == NodeType.Client)
            {
                var targetNode = NetNodeMgr.Get(node.defaultTargetUid);
                if (targetNode != null)
                {
                    (targetNode as StreamServer).GetStreamClientAgent().PlayerConnect(node.uid);
                }
            }
        }

        public static void NodeDisconnect(INetNode node)
        {
            if (node.type == NodeType.Client)
            {
                var targetNode = NetNodeMgr.Get(node.defaultTargetUid);
                if (targetNode != null)
                {
                    (targetNode as StreamServer).GetStreamClientAgent().PlayerDisconnect(node.uid);
                }
            }
        }

        public static void To(INetNode fromNode, long defaultTargetUid, int msgId, byte[] data)
        {
            //给gate
            if (MsgHanderFactory.IsGateMessage(msgId))
            {
                var hander = MsgHanderFactory.GetHander(msgId);
                hander.Run(fromNode, MessagePack.MessagePackSerializer.Deserialize<Message>(data));
                return;
            }

            //否则发往默认节点
            if (defaultTargetUid > 0)
            {
                var node = NetNodeMgr.Get(defaultTargetUid);
                if (node != null)
                {
                    Log.Debug($"转发消息到节点:{node.uid} {node.type}");
                    node.Write(fromNode.uid, msgId, data);
                }
                else //通知发送失败
                {
                    node.OnTargetNotExist();
                }
            }
        }
    }
}
