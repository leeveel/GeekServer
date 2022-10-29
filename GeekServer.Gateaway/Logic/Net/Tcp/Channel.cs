using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;

namespace GeekServer.Gateaway.Net.Tcp
{
    public class Channel
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public const string SESSIONID = "SESSIONID";
        public ConnectionContext Context { get; protected set; }
        public ProtocolReader Reader { get; protected set; }
        protected ProtocolWriter Writer { get; set; }

        public IProtocal<NetMessage> Protocol { get; protected set; }

        public Channel(ConnectionContext context, IProtocal<NetMessage> protocal)
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
            if (Context == null)
                return;
            Context.Abort();
            Reader = null;
            Writer = null;
            Context = null;
        }

        public void WriteAsync(NetMessage msg)
        {
            if (Writer != null)
                _ = Writer.WriteAsync(Protocol, msg);
        }
    }
}
