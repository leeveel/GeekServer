namespace Geek.Server.Gateway.Common
{
    public class GateSettings : BaseSetting
    {
        public int InnerTcpPort { get; set; }
        public int MaxClientCount { get; set; }
    }
}