using Bedrock.Framework.Protocols;
using Geek.Server.Core.Net.BaseHandler;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Core.Net.Tcp
{
    public class TcpChannel : INetChannel
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public const string SESSIONID = "SESSIONID";
        public ConnectionContext Context { get; protected set; }
        public ProtocolReader Reader { get; protected set; }
        protected ProtocolWriter Writer { get; set; }
        public IProtocal<Message> Protocal { get; protected set; }
        public string RemoteAddress => remoteAddress;

        Action<Message> onMessageAct;
        Action onConnectCloseAct;
        bool triggerCloseEvt = true;
        string remoteAddress;

        public TcpChannel(ConnectionContext context, IProtocal<Message> protocal, Action<Message> onMessage = null, Action onConnectClose = null)
        {
            Context = context;
            Reader = context.CreateReader();
            Writer = context.CreateWriter();
            Protocal = protocal;
            onMessageAct = onMessage;
            onConnectCloseAct = onConnectClose;
            Context.ConnectionClosed.Register(ConnectionClosed);
            remoteAddress = context.RemoteEndPoint?.ToString();
        }

        public async Task StartAsync()
        {
            try
            {
                while (Reader != null && Writer != null)
                {
                    try
                    {
                        var result = await Reader.ReadAsync(Protocal);

                        var message = result.Message;

                        if (message != null)
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

        public ValueTask Write(object msg)
        {
            return Writer.WriteAsync(Protocal, msg as Message);
        }

        public T GetData<T>(string key)
        {
            if (Context.Items.TryGetValue(key, out var v))
            {
                return (T)v;
            }
            return default;
        }

        public void SetData(string key, object v)
        {
            Context.Items[key] = v;
        }

        public void RemoveData(string key)
        {
            Context.Items.Remove(key);
        }

        public Task Close(bool triggerCloseEvt)
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
            return Task.CompletedTask;
        }
    }
}
