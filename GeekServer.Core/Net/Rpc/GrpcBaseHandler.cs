using System.Threading.Tasks;

namespace Geek.Server
{
    public abstract class GrpcBaseHandler
    {
        public long EntityId { set; get; }
        public IMessage Msg { get; set; }
        public int ServerId { get; set; }
        public abstract Task<GrpcRes> ActionAsync();
        public virtual Task<GrpcRes> InnerActionAsync()
        {
            return ActionAsync();
        }
    }
}