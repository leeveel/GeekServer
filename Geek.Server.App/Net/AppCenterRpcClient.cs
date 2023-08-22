using Geek.Server.App.Common;
using Geek.Server.Core.Center;

namespace Geek.Server.App.Net
{
    public class AppCenterRpcClient : BaseCenterRpcClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public AppCenterRpcClient(string ip, int port)
            : base(ip, port)
        {

        }

        public AppCenterRpcClient(string url)
           : base(url)
        {

        }

        public override void ConfigChanged(ConfigInfo data)
        {
            Console.WriteLine("ConfigChanged:" + data);
        }

        public override void ServerChanged(List<ServerInfo> nodes)
        {
            LOGGER.Debug("---------------------------------");
            foreach (var node in nodes)
            {
                LOGGER.Debug("ServerId:" + node.ServerId);
            }
            LOGGER.Debug("---------------------------------");
        }

        public override void HaveMessage(string eid, byte[] msg)
        {
        }
    }
}
