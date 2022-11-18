using Geek.Server.App.Common.Handler;
using Geek.Server.Core.Net.Tcp.Handler;

namespace Geek.Server.Hotfix.Role.Bag
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
