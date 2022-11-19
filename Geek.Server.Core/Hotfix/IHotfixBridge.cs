
using Geek.Server.Core.Utils;

namespace Geek.Server.Core.Hotfix
{
    public interface IHotfixBridge
    {
        ServerType BridgeType { get; }

        Task<bool> OnLoadSuccess(bool reload);

        Task Stop();
    }
}
