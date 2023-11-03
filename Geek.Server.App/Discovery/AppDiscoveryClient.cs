using Core.Discovery;
using Geek.Server.Core.Discovery;

namespace Geek.Server.App.Discovery
{
    public class AppDiscoveryClient : BaseDiscoveryClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public static AppDiscoveryClient Instance;

        public AppDiscoveryClient()
            : base(() =>
            {
                LOGGER.Info("local ip:" + Settings.Ins.LocalIp);
                return new ServerInfo
                {
                    ServerId = Settings.Ins.ServerId,
                    LocalIp = Settings.Ins.LocalIp,
                    ServerName = Settings.Ins.ServerName, 
                    InnerPort = Settings.Ins.InnerPort,
                    OuterPort = Settings.Ins.OuterPort,
                    HttpPort = Settings.Ins.HttpPort,
                    Type = ServerType.Game
                };
            })
        {
            AppDiscoveryClient.Instance = this;
        }
    }
}