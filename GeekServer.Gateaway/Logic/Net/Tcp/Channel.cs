using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System.Diagnostics.Metrics;

namespace GeekServer.Gateaway.Net.Tcp
{
    public class Channel : INetNode
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public ConnectionContext Context { get; protected set; }
        public ProtocolReader Reader { get; protected set; }
        protected ProtocolWriter Writer { get; set; }
        public IProtocal<NetMessage> Protocol { get; protected set; }

        long _uid;
        long serverId;
        public long uid { get => _uid; }
        public NodeType type { get => NodeType.Client; }
        public long defaultTargetUid => serverId;

        public Channel(ConnectionContext context, IProtocal<NetMessage> protocal, long uid)
        {
            this._uid = uid;
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


        public bool IsClose()
        {
            return Reader == null || Writer == null;
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

        //public void WriteAsync(NetMessage msg)
        //{
        //    if (Writer != null)
        //        _ = Writer.WriteAsync(Protocol, msg);
        //}
    }
}
