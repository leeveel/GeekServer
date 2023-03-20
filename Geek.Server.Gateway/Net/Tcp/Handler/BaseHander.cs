using Geek.Server.Core.Net.Tcp;
using Geek.Server.Proto;

namespace Geek.Server.Gateway.Net.Tcp.Handler
{
    public abstract class BaseHander
    {
        public virtual void Action(NetChannel conn, Message msg)
        {

        }

        protected void WriteWithStatus(NetChannel conn, Message msg, int uniId)
        {
            msg.UniId = uniId;
            conn.Write(msg);
            if (uniId > 0)
            {
                var res = new ResErrorCode
                {
                    UniId = uniId,
                    ErrCode = 0,
                    Desc = ""
                };
                conn.Write(res);
            }
        }
    }
}
