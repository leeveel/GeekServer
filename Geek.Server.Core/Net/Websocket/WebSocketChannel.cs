using Bedrock.Framework.Protocols;
using Geek.Server.Core.Net.BaseHandler;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Geek.Server.Core.Net.Websocket
{
    public class WebSocketChannel : INetChannel
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        WebSocket webSocket;
        IProtocal<Message> protocal;
        ConcurrentDictionary<string, object> datas = new();

        Action<Message> onMessageAct;
        Action onConnectCloseAct;
        bool triggerCloseEvt = true;
        public ProtocolReader Reader { get; protected set; }
        protected ProtocolWriter Writer { get; set; }
        IDuplexPipe appPipe;
        public string RemoteAddress => "";

        public WebSocketChannel(WebSocket webSocket, IProtocal<Message> protocal, Action<Message> onMessage = null, Action onConnectClose = null)
        {
            this.webSocket = webSocket;
            this.protocal = protocal;
            var pair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            Reader = pair.Transport.CreateReader();
            Writer = pair.Transport.CreateWriter();
            appPipe = pair.Application;
            onMessageAct = onMessage;
            onConnectCloseAct = onConnectClose;
        }

        public async Task Close(bool triggerCloseEvt = true)
        {
            this.triggerCloseEvt = triggerCloseEvt;
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "socketclose", CancellationToken.None);
            }
            catch
            {
            }
            finally
            {
                webSocket = null;
            }
        }

        public async Task StartAsync()
        {
            try
            {
                _ = DoSend();
                _ = DoProcessMessage();
                await DoRevice();
            }
            catch (Exception e)
            {
                LOGGER.Debug(e.Message);
            }
            finally
            {
                if (triggerCloseEvt)
                {
                    onConnectCloseAct?.Invoke();
                }
            }
        }

        async ValueTask SendMultiSegmentAsync(ReadOnlySequence<byte> buffer)
        {
            var position = buffer.Start;
            buffer.TryGet(ref position, out var prevSegment);
            while (buffer.TryGet(ref position, out var segment))
            {
                await webSocket.SendAsync(prevSegment, WebSocketMessageType.Binary, false, CancellationToken.None);
                prevSegment = segment;
            }

            // End of message frame
            await webSocket.SendAsync(prevSegment, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        ValueTask SendAsync(ReadOnlySequence<byte> buffer)
        {
            return buffer.IsSingleSegment
                ? webSocket.SendAsync(buffer.First, WebSocketMessageType.Binary, true, CancellationToken.None)
                : SendMultiSegmentAsync(buffer);
        }

        async Task DoSend()
        {
            while (!IsClose())
            {
                var result = await appPipe.Input.ReadAsync();
                var buffer = result.Buffer;

                if (result.IsCanceled)
                {
                    break;
                }

                var end = buffer.End;
                var isCompleted = result.IsCompleted;
                if (!buffer.IsEmpty)
                {
                    await SendAsync(buffer);
                }

                appPipe.Input.AdvanceTo(end);

                if (isCompleted)
                {
                    break;
                }
            }
        }

        async Task DoRevice()
        {
            while (!IsClose())
            {
                var buffer = appPipe.Output.GetMemory();

                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await Close();
                    return;
                }

                appPipe.Output.Advance(result.Count);

                var flushTask = appPipe.Output.FlushAsync();

                if (!flushTask.IsCompleted)
                {
                    await flushTask.ConfigureAwait(false);
                }
            }
        }

        async Task DoProcessMessage()
        {
            while (!IsClose())
            {
                try
                {
                    var result = await Reader.ReadAsync(protocal);
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


        public bool IsClose()
        {
            return webSocket == null || webSocket.State == WebSocketState.Closed || webSocket.State == WebSocketState.Aborted;
        }

        public T GetData<T>(string key)
        {
            if (datas.TryGetValue(key, out var v))
            {
                return (T)v;
            }
            return default(T);
        }

        public void RemoveData(string key)
        {
            datas.Remove(key, out _);
        }

        public void SetData(string key, object v)
        {
            datas[key] = v;
        }

        public ValueTask Write(object msg)
        {
            return Writer.WriteAsync(protocal, msg as Message);
        }
    }
}
