using Geek.Server.Core.Net.Http;

namespace Geek.Server.Rebalance.Handler.Http
{
    [HttpMsgMapping("getidlegate")]
    public class QueryIdleGateHandler : BaseHttpHandler
    {
        public override bool CheckSign => false;

        /// <summary>
        /// http://ip:port/game/api?command=getgate
        /// </summary> 
        public override Task<string> Action(string ip, string url, Dictionary<string, string> parameters)
        {
            var node = GatewayMgr.GetIdleGate();
            var retMsg = "";
            if (node != null)
                retMsg = $"{node.Ip}:{node.TcpPort}";
            var res = new HttpResult(HttpResult.Stauts.Success, retMsg).ToString();
            return Task.FromResult(res);
        }
    }
}
