
using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IState
    {
        DBState State { get; }

        Task ReadStateAsync();

        Task WriteStateAsync();
    }
}
