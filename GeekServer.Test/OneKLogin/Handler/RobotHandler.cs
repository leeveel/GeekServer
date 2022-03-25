using NLog;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    public abstract class RobotHandler : BaseTcpHandler
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        protected long RoleId => GetChannel().Id;

        protected Session GetChannel()
        {
            return Channel.GetAttribute(SessionManager.SESSION).Get();
        }

        public async Task<OtherAgent> GetCompAgent<OtherAgent>() where OtherAgent : IComponentAgent
        {
            return await EntityMgr.GetCompAgent<OtherAgent>(RoleId);
        }

    }
}
