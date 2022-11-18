using Geek.Server.App.Common.Handler;
using Geek.Server.Core.Net.Tcp.Handler;

namespace Geek.Server.Hotfix.Role.Bag
{
    [MsgMapping(typeof(ReqBagInfo))]
    public class ReqBagInfoHandler : RoleCompHandler<BagCompAgent>
    {
        public override async Task ActionAsync()
        {
            await Comp.GetBagInfo(Msg as ReqBagInfo);
        }
    }
}
