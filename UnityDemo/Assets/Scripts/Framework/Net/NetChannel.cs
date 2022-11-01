using Bedrock.Framework;
using Bedrock.Framework.Protocols;
using System.Threading;
using System.Threading.Tasks;

namespace Geek.Client
{
    public class NetChannel
    {
        public const string SESSIONID = "SESSIONID";
        public ConnectionContext Context { get; protected set; }
        public ProtocolReader Reader { get; protected set; }
        public ProtocolWriter Writer { get; protected set; }

        public IProtocal<NMessage> Protocol { get; protected set; }

        public NetChannel(ConnectionContext context, IProtocal<NMessage> protocal)
        {
            Context = context;
            Reader = context.CreateReader();
            Writer = context.CreateWriter();
            Protocol = protocal;
            Context.ConnectionClosed.Register(ConnectionClosed);
        }

        protected virtual void ConnectionClosed() 
        {
            UnityEngine.Debug.Log($"{Context.RemoteEndPoint?.ToString()} 服务器断开链接");
        }

        public void RemoveSessionId()
        {
            Context.Items.Remove(SESSIONID);
        }

        public void SetSessionId(long id)
        {
            Context.Items[SESSIONID] = id;
        }

        public long GetSessionId()
        {
            if (Context.Items.TryGetValue(SESSIONID, out object idObj))
                return (long)idObj;
            return 0;
        }

        public void Abort()
        {
            Context.Abort();
        }

        public ValueTask WriteAsync(NMessage msg, CancellationToken cancellationToken = default)
        {
            return Writer.WriteAsync(Protocol, msg, cancellationToken);
        }

    }
}
