using Geek.Server.Core.Serialize;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Geek.Server.Core.Net.Websocket
{
    public class WebSocketChannel : NetChannel
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        WebSocket webSocket;
        readonly Action<Message> onMessage;
        protected readonly ConcurrentQueue<Message> sendQueue = new();
        protected readonly SemaphoreSlim newSendMsgSemaphore = new(0);

        public WebSocketChannel(WebSocket webSocket, string remoteAddress, Action<Message> onMessage = null)
        {
            this.RemoteAddress = remoteAddress;
            this.webSocket = webSocket;
            this.onMessage = onMessage;
        }

        public override async void Close()
        {
            try
            {
                base.Close();
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

        public override async Task StartAsync()
        {
            try
            {
                _ = DoSend();
                await DoRevice();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                LOGGER.Error(e.Message);
            }
        }

        async Task DoSend()
        {
            while (!IsClose())
            {
                await newSendMsgSemaphore.WaitAsync();

                if (!sendQueue.TryDequeue(out var message))
                {
                    continue;
                }
                await webSocket.SendAsync(Serializer.Serialize(message), WebSocketMessageType.Binary, true, closeSrc.Token);
            }
        }

        async Task DoRevice()
        {
            var stream = new MemoryStream();
            var buffer = new ArraySegment<byte>(new byte[2048]);

            while (!IsClose())
            {
                int len = 0;
                WebSocketReceiveResult result;
                stream.SetLength(0);
                stream.Seek(0, SeekOrigin.Begin);
                do
                {
                    result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    len += result.Count;
                    stream.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                stream.Seek(0, SeekOrigin.Begin);
                var message = Serializer.Deserialize<Message>(stream);
                onMessage(message);
            }
            stream.Close();
        }

        public override void Write(Message msg)
        {
            sendQueue.Enqueue(msg);
            newSendMsgSemaphore.Release();
        }
    }
}
