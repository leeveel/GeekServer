using Geek.Server.Core.Net.Http;

namespace Server.Logic.Logic.Http
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
        /// <summary>
        /// ***正式的HttpHandler请一定设置CheckSign为True***
        /// </summary>
        public override bool CheckSign => false;

        /// <summary>
        /// http://127.0.0.1:20000/game/api?command=test
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override Task<string> Action(string ip, string url, Dictionary<string, string> parameters)
        {
            var res = new HttpTestRes
            {
                A = 100,
                B = "hello",
                TestInfo = new HttpTestRes.Info()
            };
            res.TestInfo.Age = 18;
            res.TestInfo.Name = "leeveel";
            return Task.FromResult(res.ToString());
        }
    }
}
