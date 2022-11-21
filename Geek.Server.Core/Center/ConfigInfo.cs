using MessagePack;

namespace Geek.Server.Core.Center
{
    [MessagePackObject(true)]
    public class ConfigInfo
    {
        [Key(0)]
        public string CfgId { get; set; } = "";
        [Key(1)]
        public string Describe { get; set; } = "";
        [Key(2)]
        public DateTime? ChangeTime { get; set; }
        [Key(3)]
        public string Data { get; set; } = "{}";
    }
}
