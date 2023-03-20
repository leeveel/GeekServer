using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System.Diagnostics;

namespace Geek.Server.Core.Net.Tcp
{
    public partial class NetChannel
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public const string SESSIONID = "SESSIONID";
        public ConnectionContext Context { get; protected set; }
        public ProtocolReader Reader { get; protected set; }
        protected ProtocolWriter Writer { get; set; }
        public IProtocal<NetMessage> Protocol { get; protected set; }
        Action<NetMessage> onMessageAct;
        Action onConnectCloseAct;
        bool triggerCloseEvt = true;

        public NetChannel(ConnectionContext context, IProtocal<NetMessage> protocal, Action<NetMessage> onMessage = null, Action onConnectClose = null)
        {
            Context = context;
            Reader = context.CreateReader();
            Writer = context.CreateWriter();
            Protocol = protocal;
            onMessageAct = onMessage;
            onConnectCloseAct = onConnectClose;
            Context.ConnectionClosed.Register(ConnectionClosed);
        }

        public async Task StartReadMsgAsync()
        {
            try
            {
                while (Reader != null && Writer != null)
                {
                    try
                    {
                        var result = await Reader.ReadAsync(Protocol);
                        if (result.HaveMsg)
                        {
                            onMessageAct?.Invoke(result.Message);
                        }

                        if (result.IsCompleted)
                            break;
                    }
                    catch (Exception e)
                    {
                        LOGGER.Error(e.Message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                LOGGER.Error(e.Message);
            }


        }

        public void Write(NetMessage msg)
        {
            _ = Writer.WriteAsync(Protocol, msg);
        }

        public void Write(Message msg)
        {
            Write(new NetMessage { Msg = msg, MsgId = msg.MsgId });
        }


        public bool IsClose()
        {
            return Reader == null || Writer == null;
        }

        protected void ConnectionClosed()
        {
            if (triggerCloseEvt)
                onConnectCloseAct?.Invoke();
            Reader = null;
            Writer = null;
        }

        public void Abort(bool triggerCloseEvt)
        {
            this.triggerCloseEvt = triggerCloseEvt;
            Reader = null;
            Writer = null;
            try
            {
                Context.Abort();
            }
            catch (Exception)
            {

            }
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

    }
}
