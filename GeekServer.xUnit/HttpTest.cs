using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Geek.Server.xUnit
{
    public class HttpTest : BaseUnitTest
    {
        public HttpTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task HttpPost()
        {
            DateTime startDate = DateTime.Now;
            long timestamp = startDate.Ticks;
            string token = HttpClientHelper.GetToken(timestamp);
            var data = new
            {
                token = token,
                timestamp = timestamp + "",
                //command = "online_num_query"
                command = "test"
            };

            var res = await HttpClientHelper.PostRequest<HttpTestRes>("http://192.168.0.163:20000/game/api", JsonConvert.SerializeObject(data));
            if (res.Code == "Success")
            {
                Logger.WriteLine("post success:" + res.TestInfo.Name);
            }
            else
            {
                Logger.WriteLine("failed:" + res.Msg);
            }
        }

        [Fact]
        public async Task HttpGet()
        {
            DateTime startDate = DateTime.Now;
            long timestamp = startDate.Ticks;
            string token = HttpClientHelper.GetToken(timestamp);
            var res = await HttpClientHelper.GetRequst<HttpTestRes>($"http://192.168.0.163:20000/game/api?command=test&token={token}&timestamp={timestamp}");
            if (res.Code == "Success")
            {
                Logger.WriteLine("get success:" + res.TestInfo.Name);
            }
            else
            {
                Logger.WriteLine("failed:" + res.Msg);
            }
        }

    }
}