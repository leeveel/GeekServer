using Geek.Server.Message.Login;
using NLog;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    [TcpMsgMapping(typeof(ResLogin))]
    public class ResLoginHandler : RobotHandler
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();
        public override Task ActionAsync()
        {
            ResLogin req = (ResLogin)Msg;
            LOGGER.Info($"{req.userInfo.roleName}登录成功!");
            return Task.CompletedTask;
        }
    }
}
