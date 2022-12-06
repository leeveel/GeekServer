using MessagePack;

namespace Geek.Server.Core.Center
{
    [MessagePackObject(true)]
    public class ConfigInfo
    {
        public string CfgId { get; set; } = "";
        public string Describe { get; set; } = "";
        public DateTime? ChangeTime { get; set; }
        public string Data { get; set; } = "{}";
    }
}
