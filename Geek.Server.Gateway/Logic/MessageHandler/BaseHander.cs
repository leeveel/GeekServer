using Geek.Server.Proto;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Gateway.MessageHandler
{
    public abstract class BaseHander
    {
        public virtual void Run(INetNode node, Message msg)
        {

        }

        protected void WriteMsgWithState(INetNode node, Message msg, int uniId)
        {
            msg.UniId = uniId;
            node.Write(0, msg.MsgId, MessagePack.MessagePackSerializer.Serialize(msg));
            if (uniId > 0)
            {
                ResErrorCode res = new ResErrorCode
                {
                    UniId = uniId,
                    ErrCode = 0,
                    Desc = ""
                };
                node.Write(Settings.ServerId, res.MsgId, MessagePack.MessagePackSerializer.Serialize(res));
            }
        }
    }
}
