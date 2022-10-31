using System;
using System.Buffers;
using System.Formats.Asn1;
using System.Reflection.PortableExecutable;
using System.Threading.Channels;
using System.Threading.Tasks;
using GeekServer.Gateaway.Net.Router;
using Microsoft.AspNetCore.Connections;

namespace GeekServer.Gateaway.Net.Tcp
{
    internal class TcpConnectionHandler : ConnectionHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public TcpConnectionHandler() { }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            var channel = OnConnection(connection);
            var remoteInfo = channel.Context.RemoteEndPoint;
            while (!channel.IsClose())
            {
                try
                {
                    var result = await channel.Reader.ReadAsync(channel.Protocol);
                    var message = result.Message;

                    if (result.IsCompleted)
                        break;

                    MsgDecoder.Decode(connection, ref message);
                    Dispatcher(channel, message);
                }
                catch (ConnectionResetException)
                {
                    LOGGER.Info($"{remoteInfo} ConnectionReset...");
                    break;
                }
                catch (ConnectionAbortedException)
                {
                    LOGGER.Info($"{remoteInfo} ConnectionAborted...");
                    break;
                }
                catch (Exception e)
                {
                    LOGGER.Error($"{remoteInfo} Exception:{e.Message}");
                }

                try
                {
                    channel.Reader.Advance();
                }
                catch (Exception e)
                {
                    LOGGER.Error($"{remoteInfo} channel.Reader.Advance Exception:{e.Message}");
                    break;
                }
            }
            OnDisconnection(channel);
        }

        protected Channel OnConnection(ConnectionContext connection)
        {
            LOGGER.Debug($"{connection.RemoteEndPoint?.ToString()} 链接成功");
            var channel = new Channel(connection, new MessageProtocol(), IdGenerator.GetActorID(ActorType.Role, GateSettings.Ins.ServerId));
            NetNodeMgr.Add(channel);
            return channel;
        }

        protected void OnDisconnection(Channel channel)
        {
            LOGGER.Debug($"{channel.remoteUrl} 断开链接");
            NetNodeMgr.Remove(channel);
            MsgRouter.NodeDisconnect(channel);
        }


        protected void Dispatcher(Channel channel, NetMessage msg)
        {
            MsgRouter.To(channel, channel.defaultTargetUid, msg.MsgId, msg.MsgRaw);
        }
    }
}