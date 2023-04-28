using Common.Net.Tcp;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Proto;

namespace Geek.Server.Gateway.Net.Tcp.Handler
{
    public abstract class BaseHander
    {
        public virtual void Action(INetChannel conn, Message msg)
        {

        }

        protected void Write(INetChannel conn, Message msg, int uniId)
        {
            msg.UniId = uniId;
            conn.Write(new NetMessage(msg, 0));
        }
    }
}
