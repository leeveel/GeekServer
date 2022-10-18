
namespace Geek.Server
{
    public abstract class RoleTcpHandler : BaseTcpHandler
    {
        public virtual Task InnerAction(NetChannel Channel, Message Msg)
        {
            GetActor(Channel)?.Tell(() => { ActionAsync(Channel, Msg); });
            return Task.CompletedTask;
        }
    }
}
