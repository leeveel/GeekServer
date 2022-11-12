using Bedrock.Framework.Protocols;
using Geek.Server.Proto;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Gateway.Net.Tcp
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
        public long defaultTargetUid { get => serverId; set => serverId = value; }

        public string remoteUrl;

        public Channel(ConnectionContext context, IProtocal<NetMessage> protocal, long uid)
        {
            remoteUrl = context.RemoteEndPoint.ToString();
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

        public void Write(long fromId, int msgId, byte[] data)
        {
            if (Writer != null)
                _ = Writer.WriteAsync(Protocol, new NetMessage(msgId, data));
        }

        public async void OnTargetNotExist()
        {
            await Task.Delay(500);
            //Abort();
            //通知客户端服务节点没有连接
            var res = new ServerNotConnect
            {
                serverUid = this.serverId
            };
            Write(Settings.ServerId, res.MsgId, MessagePack.MessagePackSerializer.Serialize(res));
        }
    }
}
