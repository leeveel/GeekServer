using Newtonsoft.Json;

namespace Test.Pressure
{
    public class TestSettings
    {
        public static TestSettings Ins;
        public int serverId;
        public string serverIp;
        public int serverPort;
        public int clientCount;
        public static void Load(string path)
        {
            var configJson = File.ReadAllText(path);
            Ins = JsonConvert.DeserializeObject<TestSettings>(configJson);
        }
    }
}