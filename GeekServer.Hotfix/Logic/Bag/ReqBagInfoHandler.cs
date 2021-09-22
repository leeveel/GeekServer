using Geek.Server.Logic.Handler;
using Geek.Server.Message.Bag;
using System.Threading.Tasks;

namespace Geek.Server.Logic.Bag
{
    [TcpMsgMapping(typeof(ReqBagInfo))]
    public class ReqBagInfoHandler : RoleActorHandler
    {
        public override async Task ActionAsync()
        {
            var bagComp = await Actor.GetCompAgent<BagCompAgent>();
            var msg = await bagComp.BuildInfoMsg();
            WriteAndFlush(msg);
        }
    }
}
