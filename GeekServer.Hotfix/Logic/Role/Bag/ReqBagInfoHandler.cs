
namespace Geek.Server.Role
{
    [MsgMapping(typeof(ReqBagInfo))]
    public class ReqBagInfoHandler : RoleTcpHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public override async Task ActionAsync(NetChannel channel, Message msg)
        {
            var agent = await GetActor(channel).GetCompAgent<BagCompAgent>();
            var ret = await agent.BuildInfoMsg();
            channel.WriteAsync(ret, msg.UniId);
        }
    }
}
