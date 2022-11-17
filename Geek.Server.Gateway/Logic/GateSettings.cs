namespace Geek.Server.Gateway
{
    public class GateSettings : BaseSetting
    {
        public int RpcPort { get; set; }
        public int InnerTcpPort { get; set; }
    }
}