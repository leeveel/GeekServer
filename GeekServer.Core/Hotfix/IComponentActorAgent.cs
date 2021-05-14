using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IComponentActorAgent : IAgent
    {
        Task Active();
        Task Deactive();
    }
}