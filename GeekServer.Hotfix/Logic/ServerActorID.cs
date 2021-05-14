
namespace Geek.Server
{
    public enum ActorType
    {
        Normal = 1,
        Rank,
    }

    public class ServerActorID
    {
        public static long GetID(ActorType actorType, int serverId = 0)
        {
            if (serverId <= 0)
                serverId = Settings.Ins.ServerId;
            return serverId * 1000 + (int)actorType;
        }
    }
}
