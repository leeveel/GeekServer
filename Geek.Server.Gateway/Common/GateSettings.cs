namespace Geek.Server.Gateway.Common
{
    public class GateSettings : BaseSetting
    {
        public int InnerUdpPort { get; set; }
        public int OuterPort { get; set; }  //tcp udp可以绑定同一个端口  所以外部端口可以同一个
        public int MaxClientCount { get; set; }
    }
}