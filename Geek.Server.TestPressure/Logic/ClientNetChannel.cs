using System.Buffers;
using System.Net;
using Geek.Server.Core.Net.Bedrock.Client;
using Geek.Server.Core.Net.Bedrock.Protocols;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.TestPressure.Logic
{
    public enum NetCode
    {
        TimeOut = 100,     //超时
        Success,               //连接成功
        Disconnect,           //断开连接
        Failed,                  //链接失败
    }
    public class ClientNetChannel
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        Client client;
        ConnectionContext context;
        ProtocolReader reader;
        ProtocolWriter writer;
        IProtocal<NMessage> protocol;

        public ClientNetChannel(Client client)
        {
            this.client = client;
        }

        public async Task<NetCode> Connect(string host, int port)
        {
            try
            {
                var connection = await ClientFactory.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port));
                context = connection;
                reader = context.CreateReader();
                writer = context.CreateWriter();
                protocol = new ClientLengthPrefixedProtocol();
                isConnection = true;
                _ = Task.Run(NetLooping);
                return NetCode.Success;
            }
            catch (Exception e)
            {
            Log.Error(e.Message);
                return NetCode.Failed;
            }
        }

        public void Write(NMessage msg, CancellationToken cancellationToken = default)
        {
            if (writer != null)
                _ = writer.WriteAsync(protocol, msg, cancellationToken);
        }

        void ConnectionClosed()
        {
            isConnection = false;
            client.OnDisConnected();
        }

        private bool isConnection = false;
        async Task NetLooping()
        {
            while (isConnection)
            {
                try
                {
                    var result = await reader.ReadAsync(protocol);

                    var message = result.Message;

                    Dispatcher(message);

                    if (result.IsCompleted)
                        break;
                }
                catch (ConnectionResetException)
                {
                    Log.Info($"{context.ConnectionId} disconnected");
                    break;
                }
                finally
                {
                    reader.Advance();
                }
            }
        }


        void Dispatcher(NMessage message)
        {
            var sReader = new SequenceReader<byte>(message.Payload);

            //数据包长度+消息ID=两个int=8位
            if (message.Payload.Length < 4)
                return;

            //消息id
            sReader.TryReadBigEndian(out int msgId);

            var msgType = MsgFactory.GetType(msgId);
            if (msgType == null)
            {
                Log.Error($"消息ID:{msgId} 找不到对应的Msg.");
                return;
            }
            var msg = MessagePack.MessagePackSerializer.Deserialize<Message>(message.Payload.Slice(4));
            if (msg.MsgId != msgId)
            {
                Log.Error($"后台解析消息失败，注册消息id和消息无法对应.real:{msg.MsgId}, register:{msgId}");
                return;
            }
            client.OnRevice(msg);
        }
    }
}
