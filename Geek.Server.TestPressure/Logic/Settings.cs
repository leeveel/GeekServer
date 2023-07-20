using Newtonsoft.Json;

namespace Geek.Server.TestPressure.Logic
{
    public class TestSettings
    {
        public static TestSettings Ins;
        public int serverId;
        public int clientCount;
        public string gateIP;
        public int gatePort;
        public static void Load(string path)
        {
            var configJson = File.ReadAllText(path);
            Ins = JsonConvert.DeserializeObject<TestSettings>(configJson);
        }
    }
}