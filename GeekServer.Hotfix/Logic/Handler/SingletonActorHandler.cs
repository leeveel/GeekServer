using System.Threading.Tasks;

namespace Geek.Server.Logic.Handler
{

    /// <summary>
    /// 每个服仅有一个的Actor类型
    /// </summary>
    public abstract class SingletonActorHandler : TcpActorHandler
    {
        public abstract ActorType ActorType { get; }

        public override Task<ComponentActor> GetActor()
        {
            return ActorMgr.GetOrNew(ActorType);
        }
    }
}
