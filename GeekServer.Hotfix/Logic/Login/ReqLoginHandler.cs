using Geek.Server.Message.Login;
using System.Threading.Tasks;

namespace Geek.Server.Logic.Login
{

    [MsgMapping(typeof(ReqLogin))]
    public class ReqLoginHandler : FixedIdEntityHandler<LoginCompAgent>
    {
        public override EntityType EntityType => EntityType.Login;

        public override async Task ActionAsync()
        {
            var comp = await GetCompAgent();
            var msg = await comp.Login(Channel, (ReqLogin)Msg);
            WriteAndFlush(msg);
        }
    }
}
