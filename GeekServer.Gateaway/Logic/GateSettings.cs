using Newtonsoft.Json;

namespace GeekServer.Gateaway
{
    public class GateSettings
    {
        public static GateSettings Ins;
        public bool AppRunning = false;
        public int ServerId;
        public int TcpPort;
        public int RpcPort;
        public static void Load(string path)
        {
            var configJson = File.ReadAllText(path);
            Ins = JsonConvert.DeserializeObject<GateSettings>(configJson);
        }
    }
}