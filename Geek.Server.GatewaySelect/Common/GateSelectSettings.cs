namespace Geek.Server.Gateway.Common
{
    public class GateSelectSettings : BaseSetting
    {
        public int InnerTcpPort { get; set; }
        public int MaxClientCount { get; set; }
    }
}