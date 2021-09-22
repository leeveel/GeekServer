using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class TcpActorHandler : BaseTcpHandler
    {
        public ComponentActor Actor { get; set; }
        /// <summary>在此函数中对Actor进行赋值</summary>
        public abstract Task<ComponentActor> GetActor();

        protected Session GetChannel()
        {
            return Ctx.Channel.GetAttribute(SessionManager.SESSION).Get();
        }

        protected override void WriteAndFlush(BaseMessage msg)
        {
            //将req的uniid带回去
            msg.UniId = Msg.UniId;
            base.WriteAndFlush(msg);
        }

    }
}
