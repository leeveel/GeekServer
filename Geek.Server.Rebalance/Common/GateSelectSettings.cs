namespace Geek.Server.Rebalance.Common
{
    public class GateSelectSettings : BaseSetting
    {
        public int InnerTcpPort { get; set; }
        public int MaxClientCount { get; set; }
    }
}