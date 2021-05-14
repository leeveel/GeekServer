using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IChannel
    {
        ///<summary>每次收到消息触发</summary>
        Task Hand();
        ///<summary>移除时触发</summary>
        Task OnDisconnect();
    }
}
