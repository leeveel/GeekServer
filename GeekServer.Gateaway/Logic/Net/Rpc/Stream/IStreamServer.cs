using MagicOnion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GeekServer.Gateaway.Net.Rpc
{
    //gate ->other (里面接口由客户端实现，服务器调用)
    public interface IStreamClient
    {
        public void Revice(long fromUid, byte[] data)
        {

        }
    }

    //other -> gate(里面接口由服务器实现，客户端调用)
    public interface IStreamServer : IStreamingHub<IStreamServer, IStreamClient>
    {
        //   game服 或者登陆服需要面对一对多的情况，这时候需要 实现在game或者login服 管理链接，能知道是哪个gate发过来的 GAME SEREVER(channel包含gate的server引用）

        //返回在gate中的uid
        public Task<long> SetInfo(long serverId, int type)
        {
            return Task.FromResult(0L);
        }

        //请求路由消息
        public void Router(long targetUid, int msgId, byte[] data)
        {

        }
    }
}
