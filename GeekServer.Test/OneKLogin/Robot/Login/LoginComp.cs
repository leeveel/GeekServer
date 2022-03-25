using Geek.Server.Message.Login;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    public class LoginComp : NoHotfixComponent { }


    public class LoginCompAgent : FuncComponentAgent<LoginComp>
    {
        public async Task<bool> ReqLogin()
        {
            string playerId = EntityId.ToString();
            ReqLogin msg = new ReqLogin
            {
                userName = playerId,
                platform = "unity",
                device = "device:" + playerId,
                sdkType = 0
            };
            var net = await GetCompAgent<NetCompAgent>();
            return await net.SendMsg(msg);
        }

    }
}
