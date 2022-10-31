using MagicOnion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Geek.Server
{
    //gate ->other (里面接口由客户端实现，服务器调用)
    public interface IStreamClient
    {
        public void Revice(long fromUid, int msgId, byte[] data)
        {

        }
        public void PlayerConnect(long uid)
        {

        }
        public void PlayerDisconnect(long uid)
        {

        }
    }

    //other -> gate(里面接口由服务器实现，客户端调用)
    public interface IStreamServer : IStreamingHub<IStreamServer, IStreamClient>
    {
        //返回gateid
        public Task<long> SetInfo(int serverId, int type)
        {
            return Task.FromResult(0L);
        }
        //请求断开某个节点
        public Task DisconnectNode(long targetUid)
        {
            return Task.CompletedTask;
        }
        //请求路由消息
        public Task Router(long targetUid, int msgId, byte[] data)
        {
            return Task.CompletedTask;
        }
    }
}
