using Geek.Server.Core.Net.Messages;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Proto;

namespace Geek.Server.Gateway.Net.Tcp.Handler
{
    public abstract class BaseHander
    {
        public virtual void Action(Connection conn, Message msg)
        {

        }

        protected void WriteWithStatus(Connection conn, Message msg, int uniId)
        {
            msg.UniId = uniId;
            conn.WriteAsync(new NetMessage(msg));
            if (uniId > 0)
            {
                var res = new ResErrorCode
                {
                    UniId = uniId,
                    ErrCode = 0,
                    Desc = ""
                };
                conn.WriteAsync(new NetMessage(res));
            }
        }
    }
}
