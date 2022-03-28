using Geek.Server.Logic.Role;
using Geek.Server.Proto;
using System.Threading.Tasks;

namespace Geek.Server.Logic.Bag
{
    [MsgMapping(typeof(ReqBagInfo))]
    public class ReqBagInfoHandler : TcpCompHandler<BagCompAgent>
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public override async Task ActionAsync()
        {
            var bagComp = await GetCompAgent<BagCompAgent>();
            var msg = await bagComp.BuildInfoMsg();
            //WriteAndFlush(MSG.Create(msg));
            await (await GetCompAgent<RoleCompAgent>()).NotifyClient(MSG.Create(msg, Msg.UniId));
        }
    }
}
