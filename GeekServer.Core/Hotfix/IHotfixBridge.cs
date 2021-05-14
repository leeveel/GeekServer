using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IHotfixBridge
    {
        /// <summary>加载完成回调</summary>
        Task<bool> OnLoadSucceed(bool isReload);

        /// <summary>关服接口</summary>
        Task<bool> Stop();

        /// <summary>服务器类型</summary>
        ServerType BridgeType { get; }
    }
}
