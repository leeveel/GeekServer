using Geek.Server.Core.Actors;
using Geek.Server.Core.Net.Http;
using Geek.Server.Hotfix.Server;
using NLog.Fluent;

namespace Geek.Server.Hotfix.Http
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
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
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
        public override async Task<string> Action(string ip, string url, Dictionary<string, string> parameters)
        {
            var agent = await ActorMgr.GetCompAgent<TestServerCompAgent>();

            await agent.TestCall("测试参数1", 234567);
            await agent.TestCall2(new App.Login.PlayerInfo { playerId = "playerid111", UserName = "23232323", RoleMap = new Dictionary<int, long> { { 2, 32 } } });
            await agent.TestCall3(new List<int> { 4, 5, 2, 65, 2, 32, 23 });
            var data = await agent.TestCall4();
            Log.Warn($"远程调用actor test call4结果：{MessagePack.MessagePackSerializer.SerializeToJson(data)}");
            var data2 = await agent.TestCall5();
            Log.Warn($"远程调用actor test call5结果：{MessagePack.MessagePackSerializer.SerializeToJson(data2)}");

            var res = new HttpTestRes
            {
                A = 100,
                B = "hello",
                TestInfo = new HttpTestRes.Info()
            };
            res.TestInfo.Age = 18;
            res.TestInfo.Name = "leeveel";
            // return Task.FromResult(res.ToString());
            return res.ToString();
        }
    }
}
