using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IAgent
    {
        object Owner { get; set; }
    }
}