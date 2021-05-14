using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IComponentAgent : IAgent
    {
        Task Active();
        Task Deactive();
    }
}
