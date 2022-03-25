using System.Threading.Tasks;

namespace Geek.Server
{

    /// <summary>
    /// 因为actor有自动回收 需要手动在active再判断一次是否跨天
    /// 一般来说玩家的active跨天检测可以放到登陆时
    /// </summary>
    public interface ICrossDay
    {
        Task OnCrossDay(int openServerDay);
    }
}
