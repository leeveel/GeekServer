using Geek.Server.Proto;
using System;
using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class TcpCompHandler<TAgent> : TcpCompHandler where TAgent : IComponentAgent
    {
        public override Type CompAgentType => typeof(TAgent);

        TAgent cacheAgent;
        public async Task<TAgent> GetCompAgent()
        {
            if (cacheAgent != null)
                return cacheAgent;
            cacheAgent = await EntityMgr.GetCompAgent<TAgent>(await GetEntityId());
            return cacheAgent;
        }

        public async Task<OtherAgent> GetCompAgent<OtherAgent>() where OtherAgent : IComponentAgent
        {
            return await EntityMgr.GetCompAgent<OtherAgent>(await GetEntityId());
        }

        protected virtual void NotifyErrorCode(ErrInfo errInfo)
        {
            ResErrorCode res = new ResErrorCode
            {
                UniId = Msg.UniId,  //写入req中的UniId
                ErrCode = (int)errInfo.Code,
                Desc = errInfo.Desc
            };
            NMessage msg = new NMessage(res);
            WriteAndFlush(msg);
        }

    }
}
