using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class TcpActorHandler : BaseTcpHandler
    {
        public ComponentActor Actor { get; set; }
        /// <summary>在此函数中对Actor进行赋值</summary>
        public abstract Task<ComponentActor> CacheActor();
    }
}
