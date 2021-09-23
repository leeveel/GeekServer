using Geek.Server.Message.Login;
using System.Threading.Tasks;

namespace Geek.Server.Test
{

    [TcpMsgMapping(typeof(ResErrorCode))]
    public class ResErrorCodeHandler : RobotHandler
    {
        public override async Task ActionAsync()
        {
            var agent = await Actor.GetCompAgent<NetCompAgent>();
            agent.Comp.Waiter.EndWait(Msg.UniId);
        }
    }
}
