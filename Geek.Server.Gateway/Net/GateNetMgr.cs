using Common.Net.Tcp;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Gateway.Common;
using Geek.Server.Gateway.Net.Rpc;
using Geek.Server.Gateway.Net.Tcp.Inner;
using Geek.Server.Gateway.Net.Tcp.Outer;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Gateway.Net
{
    internal class GateNetMgr
    {
        static Connections ClientConns { get; set; } = new Connections();
        static Connections ServerConns { get; set; } = new Connections();
        static TcpServer outerTcpServer;
        static TcpServer innerTcpServer;

        public static GateCenterRpcClient CenterRpcClient { get; set; }

        public static Task<bool> ConnectCenter()
        {
            CenterRpcClient = new GateCenterRpcClient(Settings.CenterUrl);
            return CenterRpcClient.Connect();
        }

        public static async Task StartTcpServer()
        {
            outerTcpServer = new TcpServer();
            await outerTcpServer.Start(Settings.TcpPort, builder => builder.UseConnectionHandler<OuterTcpConnectionHandler>());
            innerTcpServer = new TcpServer();
            await innerTcpServer.Start(Settings.InsAs<GateSettings>().InnerTcpPort, builder => builder.UseConnectionHandler<InnerTcpConnectionHandler>());
        }

        public static async Task Stop()
        {
            if (outerTcpServer != null)
                await outerTcpServer.Stop();
            if (innerTcpServer != null)
                await innerTcpServer.Stop();
            if (CenterRpcClient != null)
                await CenterRpcClient.Stop();
        }

        public static INetChannel AddServerNode(INetChannel channel)
        {
            return ServerConns.Add(channel);
        }

        public static void AddClientNode(INetChannel channel)
        {
            ClientConns.Add(channel);
        }

        public static void RemoveClientNode(long id)
        {
            var node = ClientConns.Remove(id);
            if (node != null && !node.IsClose())
            {
                node.Close();
            }
        }

        public static void RemoveServerNode(long id)
        {
            var node = ServerConns.Remove(id);
            if (node != null && !node.IsClose())
            {
                node.Close();
            }
        }

        public static INetChannel GetClientNode(long id)
        {
            var node = ClientConns.Get(id);
            if (node != null && !node.IsClose())
                return node;
            return null;
        }

        public static INetChannel GetServerNode(long id)
        {
            var node = ServerConns.Get(id);
            if (node != null && !node.IsClose())
                return node;
            return null;
        }

        public static List<long> GetAllServerNodeIds(List<long> result = null)
        {
            return ServerConns.GetAllNodeIds(result);
        }

        public static List<long> GetAllClientNodeWithTargetId(long target)
        {
            return ClientConns.GetAllNodeWithTargetId(target);
        }

        public static int GetClientConnectionCount()
        {
            return ClientConns.GetConnectionCount();
        }

    }
}
