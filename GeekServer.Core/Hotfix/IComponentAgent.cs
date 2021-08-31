using System.Threading.Tasks;

namespace Geek.Server
{
    public interface IComponentAgent
    {
        object Owner { get; set; }
        Task Active();
        /// <summary>
        /// 不对外开放,理论上没有重写的需求(Deactive如果有修改状态的逻辑不会被回存) 
        /// </summary>
        /// <returns></returns>
        //Task Deactive();
    }
}