using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System.Diagnostics;

namespace Geek.Server.Core.Net.Tcp.Codecs
{
    public class NetChannel
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public const string SESSIONID = "SESSIONID";
        public ConnectionContext Context { get; protected set; }
        public ProtocolReader Reader { get; protected set; }
        protected ProtocolWriter Writer { get; set; }
        public IProtocal<Message> Protocol { get; protected set; }
        Action<Message> onMessageAct;
        Action onConnectCloseAct;
        bool triggerCloseEvt = true;

        public NetChannel(ConnectionContext context, IProtocal<Message> protocal, Action<Message> onMessage = null, Action onConnectClose = null)
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

                        var message = result.Message;

                        onMessageAct?.Invoke(message);

                        if (result.IsCompleted)
                            break;
                    }
                    catch (Exception e)
                    {
                        // LOGGER.Error(e.Message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                LOGGER.Error(e.Message);
            }


        }

        public void Write(Message msg)
        {
            _ = Writer.WriteAsync(Protocol, msg);
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
