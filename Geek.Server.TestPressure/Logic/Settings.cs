using Newtonsoft.Json;

namespace Geek.Server.TestPressure.Logic
{
    public class TestSettings
    {
        public static TestSettings Ins;
        public int serverId;
        public string serverIp;
        public int serverPort;
        public int clientCount;
        public bool useWebSocket;
        public string webSocketServerUrl;

        public static void Load(string path)
        {
            var configJson = File.ReadAllText(path);
            Ins = JsonConvert.DeserializeObject<TestSettings>(configJson);
        }
    }
}