using Geek.Server.App.Common.Session;
using Geek.Server.Core.Net.Http;

namespace Server.Logic.Logic.Http
{
    [HttpMsgMapping("online_num_query")]
    public class HttpGetOnlinePlayerHandler : BaseHttpHandler
    {
        public override bool CheckSign => true;

        /// <summary>
        /// http://192.168.0.163:20000/game/api?command=online_num_query
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override Task<string> Action(string ip, string url, Dictionary<string, string> parameters)
        {

            var res = new HttpResult(HttpResult.Stauts.Success, $"当前在线人数:{SessionManager.Count()}").ToString();
            return Task.FromResult(res);
        }
    }
}
