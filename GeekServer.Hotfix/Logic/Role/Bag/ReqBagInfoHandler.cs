
namespace Geek.Server.Role
{
    [MsgMapping(typeof(ReqBagInfo))]
    public class ReqBagInfoHandler : RoleTcpHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public override async Task ActionAsync()
        {
            var agent = await Actor.GetCompAgent<BagCompAgent>();
            var ret = await agent.BuildInfoMsg();
            this.WriteWithErrCode(MSG.Create(ret, Msg.UniId));
        }
    }
}
