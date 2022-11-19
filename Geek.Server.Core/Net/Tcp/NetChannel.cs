using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Core.Net.Tcp
{
    public class NetChannel
    {
        public const string SESSIONID = "SESSIONID";
        public ConnectionContext Context { get; protected set; }
        public ProtocolReader Reader { get; protected set; }
        protected ProtocolWriter Writer { get; set; }

        public IProtocal<NetMessage> Protocol { get; protected set; }

        public NetChannel(ConnectionContext context, IProtocal<NetMessage> protocal)
        {
            Context = context;
            Reader = context.CreateReader();
            Writer = context.CreateWriter();
            Protocol = protocal;
            Context.ConnectionClosed.Register(ConnectionClosed);
        }

        protected virtual void ConnectionClosed()
        {
            Reader = null;
            Writer = null;
        }

        public void RemoveSessionId()
        {
            Context.Items.Remove(SESSIONID);
        }

        public bool IsClose()
        {
            return Reader == null || Writer == null;
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
            Reader = null;
            Writer = null;
        }

        public void WriteAsync(NetMessage msg)
        {
            if (Writer != null)
                _ = Task.Run(async () => await Writer.WriteAsync(Protocol, msg));
        }

        public void WriteAsync(Message msg)
        {
            WriteAsync(new NetMessage(msg));
        }

    }
}
