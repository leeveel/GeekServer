
using Geek.Server.Core.Actors;
using Geek.Server.TestPressure.Logic;
using PolymorphicMessagePack; 
using System.Net.WebSockets;
using System.Text;

namespace Geek.Server.TestPressure
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            //Console.Title = "Client";
            //using (var ws = new ClientWebSocket())
            //{
            //    await ws.ConnectAsync(new Uri("ws://localhost:6666/ws"), CancellationToken.None);
            //    var buffer = new byte[256];
            //    while (ws.State == WebSocketState.Open)
            //    {
            //        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
            //        if (result.MessageType == WebSocketMessageType.Close)
            //        {
            //            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            //        }
            //        else
            //        {
            //            Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, result.Count));
            //        }
            //    }
            //}
            //return;

            PolymorphicRegister.Load();
            PolymorphicResolver.Instance.Init();
            LogManager.Configuration = new XmlLoggingConfiguration("Configs/test_log.config");

            TestSettings.Load("Configs/test_config.json");
            var maxCount = TestSettings.Ins.clientCount;
            for (int i = 0; i < maxCount; i++)
            {
                new Client(CreateRoleId(i)).Start();
                await Task.Delay(5);
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