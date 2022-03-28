using Geek.Server.Proto;
using NLog;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    [MsgMapping(typeof(HearBeat))]
    public class ResHeartHandler : RobotHandler
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();
        public async override Task ActionAsync()
        {
            LOGGER.Info("-----收到服务器心跳-----");
            var msg = (HearBeat)Msg;
            var req = new HearBeat();
            req.TimeTick = msg.TimeTick;
            var net = await GetCompAgent<NetCompAgent>();
            _ = net.SendMsg(req);
        }
    }
}
