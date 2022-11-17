using Microsoft.AspNetCore.Connections;
using System.Net;
using Bedrock.Framework;
using NLog;
using Geek.Server.Core.Center;

namespace Geek.Server
{
    public enum NetCode
    {
        TimeOut = 100,     //超时
        Success,               //连接成功
        Disconnect,           //断开连接
        Failed,                  //链接失败
    }

    public class InnerTcpClient
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public NetChannel Channel { private set; get; }

        readonly NetNode netNode;

        public InnerTcpClient(NetNode node)
        {
            netNode = node;
        }

        public async Task<NetCode> Connect(string host, int port)
        {
            try
            {
                var connection = await ClientFactory.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port));
                OnConnection(connection);
                _ = Task.Run(NetLooping);
                return NetCode.Success;
            }
            catch (Exception e)
            {
                LOGGER.Error(e.Message);
                return NetCode.Failed;
            }
        }

        public void Write(NMessage msg)
        {
            if (Channel != null && !Channel.IsClose())
                Channel.WriteAsync(msg);
        }

        protected void OnConnection(ConnectionContext connection)
        {
            LOGGER.Debug($"{connection.RemoteEndPoint?.ToString()} 链接成功");
            Channel = new NetChannel(connection, new InnerProtocol());
        }

        protected void OnDisconnection(NetChannel channel)
        {
            LOGGER.Debug($"{channel.Context.RemoteEndPoint?.ToString()} 断开链接");
            //尝试重连
        }

        async Task NetLooping()
        {
            var remoteInfo = Channel.Context.RemoteEndPoint;
            while (!Channel.IsClose())
            {
                try
                {
                    var result = await Channel.Reader.ReadAsync(Channel.Protocol);
                    var message = result.Message;

                    if (result.IsCompleted)
                        break;

                    InnerMsgDecoder.Decode(ref message);
                    _ = Dispatcher(message);
                }
                catch (ConnectionResetException)
                {
                    LOGGER.Info($"{remoteInfo} disconnected");
                    break;
                }
                catch (Exception e)
                {
                    LOGGER.Info($"{remoteInfo} exception: {e.Message}");
                }

                try
                {
                    Channel.Reader.Advance();
                }
                catch (Exception e)
                {
                    LOGGER.Error($"{remoteInfo} channel.Reader.Advance Exception:{e.Message}");
                    break;
                }
            }
            OnDisconnection(Channel);
        }


        async Task Dispatcher(NMessage msg)
        {
            //LOGGER.Debug($"-------------收到消息{msg.MsgId} {msg.GetType()}");
            var handler = HotfixMgr.GetTcpHandler(msg.MsgId);
            if (handler == null)
            {
                LOGGER.Error($"找不到[{msg.MsgId}][{msg.GetType()}]对应的handler");
                return;
            }
            handler.TargetId = msg.TargetId;
            handler.NodeId = netNode.NodeId;
            handler.Msg = MessagePack.MessagePackSerializer.Deserialize<Message>(msg.MsgRaw);
            await handler.Init();
            await handler.InnerAction();
        }

    }
}
