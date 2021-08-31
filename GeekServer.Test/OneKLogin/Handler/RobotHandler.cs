using NLog;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    public abstract class RobotHandler : TcpActorHandler
    {

        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        public override async Task<ComponentActor> GetActor()
        {
            if (Actor != null)
                return Actor;

            var channel = GetChannel();
            if (channel != null)
                Actor = await ActorManager.GetOrNew(channel.Id);
            else
                LOGGER.Error("找不到Session:" + Actor.ActorId);

            return Actor;
        }

        public async Task OnReciveMsg(int uniId)
        {
            var net = await Actor.GetCompAgent<NetCompAgent>();
            net.ReciveMsg(uniId);
        }

    }
}
