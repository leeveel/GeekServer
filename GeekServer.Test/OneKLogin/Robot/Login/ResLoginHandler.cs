using Geek.Server.Proto;
using NLog;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    [MsgMapping(typeof(ResLogin))]
    public class ResLoginHandler : RobotHandler
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();
        public override Task ActionAsync()
        {
            ResLogin req = (ResLogin)Msg;
            LOGGER.Info($"{req.UserInfo.RoleName}登录成功!");
            return Task.CompletedTask;
        }
    }
}
