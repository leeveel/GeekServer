namespace Geek.Server.Gateway.Common
{
    public class GateSettings : BaseSetting
    {
        public int RpcPort { get; set; }
        public int InnerTcpPort { get; set; }
    }
}