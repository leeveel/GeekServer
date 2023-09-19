using Geek.Server.Core.Discovery;

namespace Geek.Server.App.Discovery
{
    public class AppDiscoveryClient : BaseDiscoveryClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public AppDiscoveryClient(string url)
            : base(url, () =>
            {
                return new ServerInfo
                {
                    ServerId = Settings.Ins.ServerId,
                    LocalIp = Settings.Ins.LocalIp,
                    InnerPort = Settings.Ins.InnerPort,
                    OuterPort = Settings.Ins.OuterPort,
                    HttpPort = Settings.Ins.HttpPort,
                    Type = ServerType.Game
                };
            })
        { }

        public override void ServerChanged(List<ServerInfo> nodes)
        {
            //LOGGER.Debug("ServerChanged:" + node.ServerId);
        }

        public override void HaveMessage(string eid, byte[] msg)
        {
        }
    }
}
