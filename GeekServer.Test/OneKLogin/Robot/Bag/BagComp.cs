using Geek.Server.Proto;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    public class BagComp : NoHotfixComponent { }

    public class BagCompAgent : FuncComponentAgent<BagComp>
    {
        public async Task<bool> ReqBagInfo()
        {
            ReqBagInfo msg = new ReqBagInfo();
            var net = await GetCompAgent<NetCompAgent>();
            return await net.SendMsg(msg);
        }
    }

}
