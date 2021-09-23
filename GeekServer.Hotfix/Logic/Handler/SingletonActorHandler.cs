using Geek.Server.Message.Login;
using System.Threading.Tasks;

namespace Geek.Server.Logic.Handler
{

    /// <summary>
    /// 每个服仅有一个的Actor类型
    /// </summary>
    public abstract class SingletonActorHandler : TcpActorHandler
    {
        public abstract ActorType ActorType { get; }

        public override Task<ComponentActor> GetActor()
        {
            return ActorMgr.GetOrNew(ActorType);
        }

        protected virtual void WriteAndFlush(MSG msg)
        {
            if (msg.MsgId > 0)
            {
                msg.msg.UniId = Msg.UniId;  //写入req中的UniId
                WriteAndFlush(msg.MsgId, msg.ByteArr);
            }
            NotifyErrorCode(msg.Info);
        }

        protected virtual void NotifyErrorCode(ErrInfo errInfo)
        {
            ResErrorCode res = new ResErrorCode
            {
                UniId = Msg.UniId,  //写入req中的UniId
                errCode = (int)errInfo.Code,
                desc = errInfo.Desc
            };
            NMessage msg = NMessage.Create(ResErrorCode.MsgId, res.Serialize());
            WriteAndFlush(msg);
        }

    }
}
