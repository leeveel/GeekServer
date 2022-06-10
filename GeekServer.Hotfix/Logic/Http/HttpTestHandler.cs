using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Logic.Http
{

    public class HttpTestRes : HttpResult
    {
        public class Info
        {
            public int Age { get; set; }
            public string Name { get; set; }
        }

        public int A { get; set; }
        public string B { get; set; }

        public Info TestInfo { get; set; }
    }


    [HttpMsgMapping("test")]
    public class HttpTestHandler : BaseHttpHandler
    {
        public override bool CheckSign => true;

        /// <summary>
        /// http://192.168.0.163:20000/game/api?command=test
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override Task<string> Action(string ip, string url, Dictionary<string, string> parameters)
        {
            //var res = new HttpResult(HttpResult.Stauts.Success, $"当前在线人数:{ServerCompAgent.OnlineNum}").ToString();
            var res = new HttpTestRes
            {
                A = 100,
                B = "hello",
                TestInfo = new HttpTestRes.Info()
            };
            res.TestInfo.Age = 18;
            res.TestInfo.Name = "leeveel";
            //var str = JsonConvert.SerializeObject(res);
            return Task.FromResult(res.ToString());
        }
    }
}
