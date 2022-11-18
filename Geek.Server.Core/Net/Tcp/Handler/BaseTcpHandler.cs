
namespace Geek.Server
{
    public abstract class BaseTcpHandler
    {

        public int GateNodeId { get; set; }

        /// <summary>
        /// 客户端网络连接id
        /// </summary>
        public long ClientConnId { get; set; }

        public Message Msg { get; set; }

        public virtual Task Init()
        {
            return Task.CompletedTask;
        }

        public abstract Task ActionAsync();

        public virtual Task InnerAction()
        {
            return ActionAsync();
        }

    }
}
