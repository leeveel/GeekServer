using Geek.Server.Proto;
using System.Threading.Tasks;

namespace Geek.Server.Test
{

    [MsgMapping(typeof(ResErrorCode))]
    public class ResErrorCodeHandler : RobotHandler
    {
        public override async Task ActionAsync()
        {
            var agent = await GetCompAgent<NetCompAgent>();
            agent.Comp.Waiter.EndWait(Msg.UniId);
        }
    }
}
