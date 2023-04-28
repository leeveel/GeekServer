using Common.Net.Tcp;

namespace Geek.Server.Core.Net.Tcp.Handler
{
    public abstract class BaseTcpHandler
    {
        public INetChannel Channel { get; set; }
        public long ClientNetId { get; set; }
        public Message Msg { get; set; }

        public abstract Task ActionAsync();

        public virtual Task InnerAction()
        {
            return ActionAsync();
        }

    }
}
