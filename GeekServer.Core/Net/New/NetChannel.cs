using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System.Threading;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class NetChannel
    {
        public const string SESSIONID = "SESSIONID";
        public ConnectionContext Context { get; }
        public ProtocolReader Reader { get; }
        public ProtocolWriter Writer { get; }

        public LengthPrefixedProtocol Protocol { get; }

        public NetChannel(ConnectionContext context)
        {
            Context = context;
            Reader = context.CreateReader();
            Writer = context.CreateWriter();
            Protocol = new LengthPrefixedProtocol();
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

        public ValueTask WriteAsync(Message msg, CancellationToken cancellationToken = default)
        {
            return Writer.WriteAsync(Protocol, msg, cancellationToken);
        }

    }
}
