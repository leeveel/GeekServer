
namespace Geek.Server
{
    public interface IHotfixBridge
    {
        ServerType BridgeType { get; }

        Task<bool> OnLoadSuccess(bool reload);

        Task Stop();
    }
}
