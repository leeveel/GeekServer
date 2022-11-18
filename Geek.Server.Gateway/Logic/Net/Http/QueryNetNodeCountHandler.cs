using Geek.Server.Gateway.Logic.Net;

namespace Geek.Server
{
    [HttpMsgMapping("net_node_count")]
    public class QueryNetNodeCountHandler : BaseHttpHandler
    {
        public override bool CheckSign => false;

        /// <summary>
        /// http://ip:port/game/api?command=net_node_count
        /// </summary> 
        public override Task<string> Action(string ip, string url, Dictionary<string, string> parameters)
        {
            var res = new HttpResult(HttpResult.Stauts.Success, $"当前在线节点数量:{GateNetMgr.GetConnectionCount()}").ToString();
            return Task.FromResult(res);
        }
    }
}
