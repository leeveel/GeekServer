
namespace Geek.Server.Server
{

    public class ServerState : CacheState
    {
        /// <summary>
        /// 世界等级
        /// </summary>
        public int WorldLevel { get; set; } = 1;

        public List<long> OnlineList { get; set; } = new List<long>();
    }

    [Comp(ActorType.Server)]
    public class ServerComp : StateComp<ServerState>
    {

    }

}
