using Geek.Server.Gateway.Logic.Net;
using Geek.Server.Proto;

namespace Geek.Server.Gateway.MessageHandler
{
    public abstract class BaseHander
    {
        public virtual void Action(Connection conn, Message msg)
        {

        }

        protected void WriteWithStatus(Connection conn, Message msg, int uniId)
        {
            msg.UniId = uniId;
            conn.WriteAsync(new NMessage(msg));
            if (uniId > 0)
            {
                var res = new ResErrorCode
                {
                    UniId = uniId,
                    ErrCode = 0,
                    Desc = ""
                };
                conn.WriteAsync(new NMessage(res));
            }
        }
    }
}
