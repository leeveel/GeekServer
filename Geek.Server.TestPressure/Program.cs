
using Geek.Server.Core.Actors;
using Geek.Server.Core.Net.Http;
using PolymorphicMessagePack;
using Geek.Server.TestPressure.Logic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NLog.Fluent;
using System.Net; 

namespace Geek.Server.TestPressure
{

    class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static async Task Main(string[] args)
        {
            PolymorphicRegister.Load();
            PolymorphicResolver.Instance.Init();

            LogManager.Configuration = new XmlLoggingConfiguration("Configs/test_log.config");
            TestSettings.Load("Configs/test_config.json");

            //通过网关选择服，获得网关地址
            try
            {
                //HttpClient client = new HttpClient();
                //var response = await client.GetAsync(TestSettings.Ins.gateSelectUrl);
                //string responseBody = await response.Content.ReadAsStringAsync();
                //var result = JsonConvert.DeserializeObject<HttpResult>(responseBody);

                var clientList = new List<Client>();

                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        lock (clientList)
                        {
                            foreach (var c in clientList)
                            {
                                c.Update();
                            }
                        }
                        await Task.Delay(3);
                    }
                });

                for (int i = 0; i < TestSettings.Ins.clientCount; i++)
                {
                    // _ = new Client(CreateRoleId(i)).Start(TestSettings.Ins.gateIP, TestSettings.Ins.gatePort);
                    var client = new Client(i); //
                    lock (clientList)
                    {
                        _ = client.Start(TestSettings.Ins.gateIP, 7899);
                        clientList.Add(client);
                    }
                    await Task.Delay(1);
                }


            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            Console.ReadLine();
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