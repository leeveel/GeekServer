using Geek.Server.App.Net.Session;
using Geek.Server.Core.Net.Tcp.Handler;

namespace Server.Logic.Gate
{
    [MsgMapping(typeof(PlayerDisconnected))]
    public class PlayerDisconnctedHandler : BaseTcpHandler
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public override Task ActionAsync()
        {
            //var msg = Msg as PlayerDisconnected;
            //此处可以获得网关节点id, msg.GateId
            SessionManager.RemoveByClientConnId(ClientConnId);
            LOGGER.Debug($"客户端掉线:{ClientConnId}");
            return Task.CompletedTask;
        }
    }
}
