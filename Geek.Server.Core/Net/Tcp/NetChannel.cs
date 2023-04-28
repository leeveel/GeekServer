using Bedrock.Framework.Protocols;
using Common.Net.Tcp;
using Microsoft.AspNetCore.Connections;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Geek.Server.Core.Net.Tcp
{
    public class NetChannel<IMessage> : INetChannel where IMessage : class
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public ConnectionContext Context { get; set; }
        public ProtocolReader Reader { get; set; }
        IProtocal<IMessage> Protocol { get; set; }
        ProtocolWriter Writer { get; set; }
        public long NetId { get; set; } = 0;
        public long DefaultTargetNodeId { get; set; }
        public long ResCode { get; set; }
        public string RemoteAddress { get => Context?.RemoteEndPoint.ToString(); }
        ConcurrentDictionary<string, object> Datas { get; set; } = new();
        Func<IMessage, Task> onMessageAct;
        Action onConnectCloseAct;

        public NetChannel(ConnectionContext context, IProtocal<IMessage> protocal, Func<IMessage, Task> onMessage = null, Action onConnectClose = null)
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
            while (Reader != null && Writer != null)
            {
                try
                {
                    var result = await Reader.ReadAsync(Protocol);
                    if (result.HaveMsg)
                    {
                        if (onMessageAct != null)
                            await onMessageAct(result.Message);
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

        public async ValueTask Write(object msg)
        {
            var realMsg = msg as IMessage;
            if (realMsg == null)
            {
                LOGGER.Error($"写入错误的消息类型,需要:{typeof(IMessage).FullName},实际是:{msg.GetType().FullName}");
            }
            await Writer.WriteAsync(Protocol, realMsg);
        }

        public bool IsClose()
        {
            return Reader == null || Writer == null;
        }

        protected void ConnectionClosed()
        {
            onConnectCloseAct?.Invoke();
            Reader = null;
            Writer = null;
        }

        public void Close()
        {
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

        public T GetData<T>(string key)
        {
            if (Datas.TryGetValue(key, out var v))
            {
                return (T)v;
            }
            return default(T);
        }

        public void SetData(string key, object v)
        {
            Datas[key] = v;
        }
    }
}
