using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IDeadable
    {
        Task Dieout();
    }
}
