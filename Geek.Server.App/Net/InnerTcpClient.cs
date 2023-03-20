using Microsoft.AspNetCore.Connections;
using System.Net;
using Bedrock.Framework;
using NLog;
using Geek.Server.Core.Center;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Net;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Core.Net.Tcp.Inner;
using System.Net.Sockets;

namespace Geek.Server.App.Net
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
        private Func<Message> regMsgGetter;
        private ReConnecter reConn;

        public InnerTcpClient(NetNode node)
        {
            netNode = node;
            reConn = new ReConnecter(ConnectImpl, $"网关服{node.Ip}:{node.InnerTcpPort}");
        }

        public Task<bool> Connect()
        {
            return reConn.Connect();
        }

        public void Register(Func<Message> regMsgGetter)
        {
            this.regMsgGetter = regMsgGetter;
            if (regMsgGetter != null)
                Write(regMsgGetter());
        }

        public void TryReConnectImmediately()
        {
            _ = reConn.TryReConnectImmediately();
        }

        public async Task<bool> ConnectImpl()
        {
            try
            {
                var connection = await new SocketConnection(AddressFamily.InterNetwork, netNode.Ip, netNode.InnerTcpPort).StartAsync(10000);
                if (connection != null)
                {
                    OnConnection(connection);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                LOGGER.Error(e.Message);
                return false;
            }
        }

        public void Write(NetMessage msg)
        {
            Channel?.Write(msg);
        }

        public void Write(Message msg)
        {
            Write(new NetMessage { Msg = msg,MsgId = msg.MsgId });
        }

        protected void OnConnection(ConnectionContext connection)
        {
            LOGGER.Debug($"{connection.RemoteEndPoint?.ToString()} 链接成功");
            Channel = new NetChannel(connection, new InnerProtocol(false), (netMsg) => _ = Dispatcher(netMsg), OnDisconnection);
            if (regMsgGetter != null)
            {
                Write(regMsgGetter());
            }
            _ = Channel.StartReadMsgAsync();
        }

        protected void OnDisconnection()
        {
            LOGGER.Debug($"{Channel.Context.RemoteEndPoint?.ToString()} 断开链接");
            //尝试重连
            _ = reConn.ReConnect();
        }

        async Task Dispatcher(NetMessage nMsg)
        {
            var handler = HotfixMgr.GetTcpHandler(nMsg.MsgId);
            if (handler == null)
            {
                LOGGER.Error($"找不到[{nMsg.MsgId}]对应的handler");
                return;
            }
            handler.ClientConnId = nMsg.NetId;
            handler.Channel = Channel;
            handler.Msg = nMsg.Msg;
            await handler.Init();
            await handler.InnerAction();
        }

    }
}
