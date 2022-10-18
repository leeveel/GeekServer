
namespace Geek.Server
{
    public abstract class BaseTcpHandler
    {
        public Actor GetActor(NetChannel Channel)
        {
            var actorId = Channel.GetSessionId();
            if (actorId > 0)
                return ActorMgr.GetActor(actorId);
            return null;
        }
        public abstract Task ActionAsync(NetChannel Channel, Message Msg);
    }
}
