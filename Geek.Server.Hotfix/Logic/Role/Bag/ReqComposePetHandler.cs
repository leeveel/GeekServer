using Geek.Server.Core.Net.Tcp.Handler;
using Geek.Server.Proto;

namespace Server.Logic.Logic.Role.Bag
{
    [MsgMapping(typeof(ReqComposePet))]
    public class ReqComposePetHandler : RoleCompHandler<BagCompAgent>
    {
        public override async Task ActionAsync()
        {
            await Comp.ComposePet(Msg as ReqComposePet);
        }
    }
}
