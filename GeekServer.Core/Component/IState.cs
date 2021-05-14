using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IState
    {
        DBState State { get; }

        Task ReadStateAsync();

        Task WriteStateAsync();

        Task ReloadState(int coldTimeInMinutes = 30);
    }
}
