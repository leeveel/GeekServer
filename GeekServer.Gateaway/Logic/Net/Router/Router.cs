using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.Net.Router
{
    public class MsgRouter
    {
        public static void To(INetNode fromNode, long defaultTargetUid, int msgId, byte[] data)
        {
            //给gate
            //if ()
            //{
            //    return;
            //}
            //给登录服


            //否则发往默认节点
            if (defaultTargetUid > 0)
            {
                var node = NetNodeMgr.Get(defaultTargetUid);
                if (node != null)
                {
                    node.Write(fromNode.uid, msgId, data);
                }
                else //通知发送失败
                {

                }
            }
        }
    }
}
