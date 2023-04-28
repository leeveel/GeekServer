using Microsoft.AspNetCore.Connections;
using System.Net;
using Bedrock.Framework;
using NLog;
using Geek.Server.Core.Center;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Net;
using Geek.Server.Core.Net.Tcp;
using System.Net.Sockets;
using Common.Net.Tcp;
using System.Collections.Concurrent;
using Geek.Server.Core.Utils;
using Geek.Server.Proto;
using Geek.Server.App.Net.Session;
using Geek.Server.App.Common.Handler;
using Geek.Server.Core.Net.Tcp.Handler;

namespace Geek.Server.App.Net
{

    public class InnerTcpClient
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public NetChannel<Message> Channel { private set; get; }
        private ReConnecter reConn;
        private Func<Message> regMsgGetter;
        private string Ip;
        private int port;
        private ConcurrentDictionary<long, INetChannel> netProxies = new();
        private bool isClose;

        public InnerTcpClient(string Ip, int port)
        {
            this.Ip = Ip;
            this.port = port;
            reConn = new ReConnecter(ConnectImpl, $"网关服{Ip}:{port}");
        }

        public void AddProxy(long netId, INetChannel proxy)
        {
            netProxies[netId] = proxy;
        }

        public INetChannel GetProxy(long netId)
        {
            netProxies.TryGetValue(netId, out var p);
            return p;
        }

        public INetChannel RemoveProxy(long netId)
        {
            netProxies.TryRemove(netId, out var p);
            return p;
        }

        public void SyncNetProxies(List<long> ids)
        {
            var removeIds = new List<long>();
            foreach (var p in netProxies)
            {
                if (!ids.Contains(p.Key))
                {
                    removeIds.Add(p.Key);
                }
            }
            foreach (var id in removeIds)
            {
                GetProxy(id)?.Close();
            }
        }


        public Task<bool> Connect()
        {
            return reConn.Connect();
        }

        public void Close()
        {
            isClose = true;
            reConn.Close();
            Channel.Close();
        }

        public void Register(Func<Message> regMsgGetter)
        {
            this.regMsgGetter = regMsgGetter;
            if (regMsgGetter != null)
                _ = Write(regMsgGetter());
        }

        public void TryReConnectImmediately()
        {
            if (Channel.IsClose())
                _ = reConn.TryReConnectImmediately();
        }

        public async Task<bool> ConnectImpl()
        {
            try
            {
                if (isClose)
                    return false;
                var connection = await new SocketConnection(AddressFamily.InterNetwork, Ip, port).StartAsync(10000);
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

        public async Task Write(object msg)
        {
            await Channel.Write(msg);
        }

        protected void OnConnection(ConnectionContext connection)
        {
            if (isClose)
            {
                try
                {
                    connection.Abort();
                    return;
                }
                catch (Exception)
                {

                }
            }
            LOGGER.Debug($"{connection.RemoteEndPoint?.ToString()} 链接成功");
            Channel = new NetChannel<Message>(connection, new InnerProtocol(), Dispatcher, OnDisconnection);
            if (regMsgGetter != null)
            {
                _ = Write(regMsgGetter());
            }
            _ = Channel.StartReadMsgAsync();
        }

        protected void OnDisconnection()
        {
            LOGGER.Debug($"{Channel.Context.RemoteEndPoint?.ToString()} 断开链接");
            //尝试重连
            if (!isClose)
                _ = reConn.ReConnect();
        }

        async Task Dispatcher(Message msg)
        {
            LOGGER.Debug($"处理消息:{msg.GetType().Name}");
            var handler = HotfixMgr.GetTcpHandler(msg.MsgId);
            if (handler == null)
            {
                LOGGER.Error($"找不到[{msg.MsgId}]对应的handler");
                return;
            }

            netProxies.TryGetValue(msg.NetId, out var proxy);
            handler.Channel = proxy;
            handler.Msg = msg;
            handler.ClientNetId = msg.NetId;

            if (handler is BaseGatewayHandler gatehandler)
            {
                gatehandler.innerTcpClient = this;
                await gatehandler.ActionAsync();
                return;
            }

            if (proxy == null)
            {
                LOGGER.Error($"handler proxy 为空 {msg.MsgId} {handler.GetType()}");
                await Channel.Write(new ReqDisconnectClient { NetId = msg.NetId });
                return;
            }


            if (handler is BaseRoleCompHandler)
            {
                if (proxy.GetData<GameSession>(SessionManager.SESSION) == null)
                {
                    LOGGER.Error($"handler game session 为空 {msg.MsgId} {handler.GetType()}");
                    await Channel.Write(new ReqDisconnectClient { NetId = msg.NetId });
                    return;
                }
            }

            if (handler is BaseCompHandler compHander)
            {
                await compHander.InitActor();
            }

            _ = handler.InnerAction();
        }

    }
}
