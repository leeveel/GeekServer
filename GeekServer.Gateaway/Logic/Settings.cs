using Newtonsoft.Json;

namespace GeekServer.Gateaway
{
    public class Settings
    {
        public static Settings Ins;
        public bool AppRunning = false;
        public int TcpPort;
        public static void Load(string path)
        {
            var configJson = File.ReadAllText(path);
            Ins = JsonConvert.DeserializeObject<Settings>(configJson);
        }
    }
}