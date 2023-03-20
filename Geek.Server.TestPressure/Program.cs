
using Geek.Server.Core.Actors;
using Geek.Server.Core.Net.Http;
using PolymorphicMessagePack;
using Geek.Server.TestPressure.Logic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NLog.Fluent;
using System.Net;
using Protocol;

namespace Geek.Server.TestPressure
{

    class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static async Task Main(string[] args)
        {
            PolymorphicRegister.Load();
            PolymorphicResolver.Init();

            LogManager.Configuration = new XmlLoggingConfiguration("Configs/test_log.config");
            TestSettings.Load("Configs/test_config.json");

            //通过网关选择服，获得网关地址
            try
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(TestSettings.Ins.gateSelectUrl);
                string responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<HttpResult>(responseBody);

                if (result.Msg != "")
                {
                    var maxCount = TestSettings.Ins.clientCount;
                    var ipEnd = IPEndPoint.Parse(result.Msg);
                    for (int i = 0; i < maxCount; i++)
                    {
                        _ = new Client(CreateRoleId(i)).Start(ipEnd.Address.ToString(), ipEnd.Port);
                        await Task.Delay(5);
                    }
                }
                else
                {
                    Log.Error("从网关选择服获取网关失败!");
                }

            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            Console.ReadLine();
        }
        private static long CreateRoleId(int index)
        {
            long actorType = (long)ActorType.Role;
            long res = (long)666 << 46;//(63-17) 
            res |= actorType << 42; //(63-4-17) 
            return res | (long)index;
        }
    }
}